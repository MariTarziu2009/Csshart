using Wester.cc;
using Swed64;
using System.Numerics;
using System.Net.Http.Headers;

// main logic

//init swed
Swed swed = new Swed("cs2");

// get client module
IntPtr client = swed.GetModuleBase("client.dll");

// init render
Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();

// get screen size from renderer
Vector2 screenSize = renderer.screenSize;

// store enities
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

// offsets

// offsets.cs <-- these normally update every day | https://github.com/a2x/cs2-dumper/blob/main/generated/offsets.hpp
int dwEntityList = 0x18B3FA8;
int dwViewMatrix = 0x19154C0;
int dwLocalPlayerPawn = 0x1729348;

// client.dll.cs | https://github.com/a2x/cs2-dumper/blob/main/generated/client.dll.cs
int m_vOldOrigin = 0x127C;
int m_iTeamNum = 0x3CB;
int m_lifeState = 0x338;
int m_hPlayerPawn = 0x7E4;
int m_vecViewOffset = 0xC58;

// ESP loop
while (true)
{
    // clean list
    entities.Clear();

    // get entity list
    IntPtr entityList = swed.ReadPointer(client, dwEntityList);

    // make entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // get local player
    IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);

    // get team (so we can compare with other entities)
    localPlayer.team = swed.ReadInt(localPlayerPawn, m_iTeamNum);

    // loop through entity list
    for (int i = 0; i < 64; i++)
    {
        // get current controller
        IntPtr currentController = swed.ReadPointer(listEntry, i = 0x78);
        if (currentController == IntPtr.Zero) continue; // check

        // get paw handle
        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if(pawnHandle == 0) continue;

        // get current pawn, make secound entry
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 0) + 0x10);
        if (listEntry2 == IntPtr.Zero) continue;

        // get current pawn
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) continue;

        // check if lifestat
        int lifestate = swed.ReadInt(currentPawn, m_lifeState);
        if (lifestate == 256) continue;

        // get matrix
        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        // popular entity
        Entity entity = new Entity();

        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);

        entities.Add(entity);
    }
    // update renderer
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);
}