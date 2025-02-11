#include <glad/glad.h>

#define SDL_MAIN_HANDLED
#include <SDL2/SDL.h>
#include <SDL2/SDL_opengles2.h>

#if __EMSCRIPTEN__
#include <emscripten.h>
#include <emscripten/html5.h>
#endif

#include <stdlib.h>

#ifndef EMSCRIPTEN_KEEPALIVE
#define EMSCRIPTEN_KEEPALIVE
#endif

static SDL_Window* window = nullptr;
static bool running = true;

const char* vertexShaderSource = R"(
attribute vec4 position;
attribute vec2 texcoord;
varying vec2 v_texcoord;

void main() {
    gl_Position = position;
    v_texcoord = texcoord;
}
)";

const char* fragmentShaderSource = R"(
precision mediump float;
varying vec2 v_texcoord;
uniform sampler2D texture;

void main() {
    gl_FragColor = texture2D(texture, v_texcoord);
}
)";

static GLuint CompileShader(GLenum type, const char* source)
{
    GLuint shader = glCreateShader(type);
    glShaderSource(shader, 1, &source, NULL);
    glCompileShader(shader);
    return shader;
}

static GLuint CreateProgram(const char* vertexSource, const char* fragmentSource)
{
    GLuint program = glCreateProgram();
    GLuint vertexShader = CompileShader(GL_VERTEX_SHADER, vertexSource);
    GLuint fragmentShader = CompileShader(GL_FRAGMENT_SHADER, fragmentSource);
    glAttachShader(program, vertexShader);
    glAttachShader(program, fragmentShader);
    glLinkProgram(program);
    return program;
}

static void GenerateCheckerboard(SDL_Surface* surface)
{
    const int checkerSize = 8;

    SDL_LockSurface(surface);
    uint32_t* pixels = (uint32_t*) surface->pixels;
    for (int y = 0; y < surface->h; ++y)
    {
        for (int x = 0; x < surface->w; ++x)
        {
            int checker = ((x / checkerSize) % 2 == (y / checkerSize) % 2) ? 0xFFFFFFFF : 0xFF000000;
            pixels[y * surface->w + x] = checker;
        }
    }
    SDL_UnlockSurface(surface);
}

static void RequestQuit()
{
    running = false;

#if __EMSCRIPTEN__
    emscripten_cancel_main_loop();
#endif
}

static void MainLoop()
{
    SDL_Event event{};
    while (SDL_PollEvent(&event))
    {
        if (event.type == SDL_QUIT)
        {
            RequestQuit();
        }
    }

    glClear(GL_COLOR_BUFFER_BIT);
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);
    SDL_GL_SwapWindow(window);
}

extern "C"
{
    EMSCRIPTEN_KEEPALIVE
    void StartRendering()
    {
        running = true;
    }

    EMSCRIPTEN_KEEPALIVE
    void StopRendering()
    {
        RequestQuit();
    }
}

int main([[maybe_unused]] int argc, [[maybe_unused]] char* argv[])
{
    SDL_SetHint(SDL_HINT_EMSCRIPTEN_KEYBOARD_ELEMENT, "#canvas");
    SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS);

    window = SDL_CreateWindow("Point Cloud Viewer Native", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 800, 600, SDL_WINDOW_OPENGL);
    SDL_GLContext glContext = SDL_GL_CreateContext(window);
    gladLoadGLES2Loader(SDL_GL_GetProcAddress);

    GLuint program = CreateProgram(vertexShaderSource, fragmentShaderSource);
    glUseProgram(program);

    GLfloat vertices[4][4] = {
        {-1.0f, 1.0f,  0.0f, 0.0f},
        {1.0f,  1.0f,  1.0f, 0.0f},
        {1.0f,  -1.0f, 1.0f, 1.0f},
        {-1.0f, -1.0f, 0.0f, 1.0f},
    };

    GLuint indices[] = {0, 1, 2, 2, 3, 0};

    GLuint vertexBuffer;
    glGenBuffers(1, &vertexBuffer);
    GLuint indexBuffer;
    glGenBuffers(1, &indexBuffer);

    glBindBuffer(GL_ARRAY_BUFFER, vertexBuffer);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

    GLint positionAttrib = glGetAttribLocation(program, "position");
    glVertexAttribPointer(positionAttrib, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(GLfloat), (const void*) 0);
    glEnableVertexAttribArray(positionAttrib);

    GLint texcoordAttrib = glGetAttribLocation(program, "texcoord");
    glVertexAttribPointer(texcoordAttrib, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(GLfloat), (const void*) (2 * sizeof(GLfloat)));
    glEnableVertexAttribArray(texcoordAttrib);

    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBuffer);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

    SDL_Surface* checkerboardSurface = SDL_CreateRGBSurface(0, 64, 64, 32, 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);
    GenerateCheckerboard(checkerboardSurface);

    GLuint texture;
    glGenTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, texture);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 64, 64, 0, GL_RGBA, GL_UNSIGNED_BYTE, checkerboardSurface->pixels);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

#if __EMSCRIPTEN__
    emscripten_set_main_loop(MainLoop, 0, 1);
#else
    while (running)
    {
        MainLoop();
    }
#endif

    SDL_FreeSurface(checkerboardSurface);
    glDeleteTextures(1, &texture);
    glDeleteProgram(program);
    glDeleteBuffers(1, &vertexBuffer);
    glDeleteBuffers(1, &indexBuffer);

    SDL_GL_DeleteContext(glContext);
    SDL_DestroyWindow(window);
    SDL_Quit();
}
