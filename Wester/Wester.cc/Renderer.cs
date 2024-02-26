using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace Wester.cc
{
    public class Renderer : Overlay 
    {
        public Vector2 screenSize = new Vector2 (1024, 768); //In here you put your screen size, my screen size is (1024, 768)

        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity> ();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        private bool enableESP = true;
        private Vector4 enemyColor = new Vector4(1, 0, 0, 1);
        private Vector4 teamColor = new Vector4(0, 1, 0, 1);

        ImDrawListPtr drawList;

        protected override void Render()
        {
            ImGui.Begin("Wester.cc");
            ImGui.Checkbox("Enable ESP", ref enableESP);
            // team color
            if (ImGui.CollapsingHeader("Team color"))
                ImGui.ColorPicker4("##teamcolor", ref teamColor);
            // enemy color
            if (ImGui.CollapsingHeader("Enemy color"))
                ImGui.ColorPicker4("##teamcolor", ref enemyColor);

            // draw overlay
            DrawOverlay(screenSize);
            drawList = ImGui.GetWindowDrawList();

            // draw stuff
            if (enableESP)
            {
                foreach (var entity in entities)
                {
                    // check if entity on screen
                    if (EnitiyOnScreen(entity))
                    {
                        DrawBox(entity);
                        DrawLine(entity);
                    }
                }
            }      
        }

        // check position
        bool EnitiyOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }

        // drawing methods

        private void DrawBox(Entity entity)
        {
            // calculate box height
            float entityHeight = entity.position2D.Y - entity.position2D.Y;

            // calculate box dimensions
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);

            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            //get correct color
            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }
        private void DrawLine(Entity entity)
        {
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        // transfer entity methods

        public void UpdateEntities(IEnumerable<Entity> newEnities)
        {
            entities = new ConcurrentQueue<Entity>(newEnities);
        }
        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }
        public Entity GetLocalPlayer()
        {
            lock (entityLock)
            {
                return localPlayer;
            }
        }
        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}
