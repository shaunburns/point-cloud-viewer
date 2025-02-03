#define SDL_MAIN_HANDLED
#include <SDL2/SDL.h>
// #include <SDL2/SDL_opengles2.h>

#include "GL/glew.h"

#if __EMSCRIPTEN__
#include <emscripten.h>
#include <emscripten/html5.h>
#endif

#include <stdlib.h>

#ifndef EMSCRIPTEN_KEEPALIVE
#define EMSCRIPTEN_KEEPALIVE
#endif

SDL_Window* window = nullptr;
SDL_Renderer* renderer = nullptr;
SDL_GLContext glContext = nullptr;
GLuint shaderProgram = 0;
bool running = true;
int width = 800;
int height = 600;
float angle = 0.0f;
bool quit = false;

const char* vertexShaderSource = R"(
    attribute vec3 aPos;
    void main() {
        gl_Position = vec4(aPos, 1.0);
    }
)";

const char* fragmentShaderSource = R"(
    precision mediump float;
    void main() {
        gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
)";

GLfloat vertices[] = {0.0f, 1.0f, 0.0f, -1.0f, -1.0f, 0.0f, 1.0f, -1.0f, 0.0f};

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
        running = false;
    }
}

void UpdateViewport(int newWidth, int newHeight)
{
    width = newWidth;
    height = newHeight;
    glViewport(0, 0, width, height);
}

void HandleEvent(SDL_Event& event)
{
    if (event.type == SDL_WINDOWEVENT)
    {
        if (event.window.event == SDL_WINDOWEVENT_RESIZED)
        {
            UpdateViewport(event.window.data1, event.window.data2);
        }
    }
    else if (event.type == SDL_QUIT)
    {
        quit = true;
    }
}

static void Update()
{
    SDL_Event event{};
    while (SDL_PollEvent(&event))
    {
        HandleEvent(event);
    }

    angle += 0.01f;
}

static void Render()
{
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glUseProgram(shaderProgram);

    glLoadIdentity();
    glTranslatef(0.0f, 0.0f, -5.0f);
    glRotatef(angle, 0.0f, 1.0f, 0.0f);

    GLuint vbo;
    glGenBuffers(1, &vbo);
    glBindBuffer(GL_ARRAY_BUFFER, vbo);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

    GLint posAttrib = glGetAttribLocation(shaderProgram, "aPos");
    glEnableVertexAttribArray(posAttrib);
    glVertexAttribPointer(posAttrib, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(GLfloat), 0);

    glDrawArrays(GL_TRIANGLES, 0, 3);

    SDL_GL_SwapWindow(window);
}

static GLuint CompileShader(GLenum type, const char* source)
{
    GLuint shader = glCreateShader(type);
    glShaderSource(shader, 1, &source, nullptr);
    glCompileShader(shader);

    GLint compiled = 0;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
    if (compiled != GL_TRUE)
    {
        char infoLog[512];
        glGetShaderInfoLog(shader, 512, nullptr, infoLog);
        SDL_LogError(SDL_LOG_CATEGORY_APPLICATION, "Shader Compilation Error: %s", infoLog);
        return 0;
    }

    return shader;
}

static GLuint CreateProgram()
{
    GLuint vertexShader = CompileShader(GL_VERTEX_SHADER, vertexShaderSource);
    GLuint fragmentShader = CompileShader(GL_FRAGMENT_SHADER, fragmentShaderSource);

    GLuint program = glCreateProgram();
    glAttachShader(program, vertexShader);
    glAttachShader(program, fragmentShader);
    glLinkProgram(program);

    GLint linked = 0;
    glGetProgramiv(program, GL_LINK_STATUS, &linked);
    if (linked != GL_TRUE)
    {
        char infoLog[512];
        glGetProgramInfoLog(program, 512, nullptr, infoLog);
        SDL_LogError(SDL_LOG_CATEGORY_APPLICATION, "Program Linking Error: %s", infoLog);
        return 0;
    }

    glDeleteShader(vertexShader);
    glDeleteShader(fragmentShader);
    return program;
}

static void CreateGlContext()
{
    glContext = SDL_GL_CreateContext(window);
    SDL_GL_MakeCurrent(window, glContext);

    GLenum GlewError = glewInit();
    if (GlewError != GLEW_OK)
    {
        SDL_LogCritical(SDL_LOG_CATEGORY_APPLICATION, "Failed to initialize GLEW: %s", glewGetErrorString(GlewError));
        return;
    }

    shaderProgram = CreateProgram();

    glEnable(GL_DEPTH_TEST);
}

static void DestroyGlContext()
{
    glDeleteProgram(shaderProgram);
    shaderProgram = 0;

    SDL_GL_DeleteContext(glContext);
    glContext = nullptr;
}

static void CreateWindowRenderer()
{
    SDL_CreateWindowAndRenderer(width, height, SDL_WINDOW_OPENGL | SDL_WINDOW_RESIZABLE, &window, &renderer);
}

static void DestroyWindowRenderer()
{
    SDL_DestroyRenderer(renderer);
    renderer = nullptr;
    SDL_DestroyWindow(window);
    window = nullptr;
}

static void MainLoop()
{
    if (!running)
        return;

    Update();
    Render();
}

int main([[maybe_unused]] int argc, [[maybe_unused]] char* argv[])
{
    SDL_Init(SDL_INIT_VIDEO);
    SDL_GL_LoadLibrary(nullptr);

    // Disable keyboard capture so that other elements on the webpage can receive input
    // SDL_EventState(SDL_TEXTINPUT, SDL_DISABLE);
    // SDL_EventState(SDL_KEYDOWN, SDL_DISABLE);
    // SDL_EventState(SDL_KEYUP, SDL_DISABLE);

    CreateWindowRenderer();
    CreateGlContext();

#if __EMSCRIPTEN__
    emscripten_set_main_loop(MainLoop, 0, 1);
#else
    while (!quit)
    {
        MainLoop();
    }
#endif

    DestroyGlContext();
    DestroyWindowRenderer();

    SDL_Quit();
    return 0;
}
