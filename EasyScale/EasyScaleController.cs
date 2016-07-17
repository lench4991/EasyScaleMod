﻿using System;
using System.Collections.Generic;

namespace Lench.EasyScale
{
    public class EasyScaleController : SingleInstance<EasyScaleController>
    {
        public override string Name { get; } = "Easy Scale Controller";

        internal List<Guid> LoadedCylinderFix = new List<Guid>();

        private bool MachineLoaded = false;

        internal void OnMachineSave(MachineInfo info)
        {
            foreach (var blockinfo in info.Blocks.FindAll(b => b.ID == (int)BlockType.Brace))
            {
                var block = Machine.Active().BuildingBlocks.Find(b => b.Guid == blockinfo.Guid);
                blockinfo.BlockData.Write("esc-cylinder-fix", (block.Toggles.Find(toggle => toggle.Key == "cylinder-fix").IsActive));
            }
        }

        internal void OnMachineLoad(MachineInfo info)
        {
            LoadedCylinderFix = new List<Guid>();
            foreach (var blockinfo in info.Blocks.FindAll(b => b.ID == (int)BlockType.Brace))
            {
                if (blockinfo.BlockData.HasKey("esc-cylinder-fix") &&
                    blockinfo.BlockData.ReadBool("esc-cylinder-fix"))
                    LoadedCylinderFix.Add(blockinfo.Guid);
            }

            MachineLoaded = true;
        }

        private void Update()
        {
            if (MachineLoaded)
            {
                EasyScale.AddAllSliders();
                EasyScale.FixAllCylinders();
                MachineLoaded = false;
            }
        }
    }
}