﻿using System.Collections.Generic;
using spaar.ModLoader;
using System.Reflection;
using UnityEngine;
// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleNullReferenceException

namespace Lench.EasyScale
{
    public class Controller : SingleInstance<Controller>
    {
        public override string Name { get; } = "Easy Scale";

        private bool _movingAllSliders;
        private int _currentBlockType = 1;
        private PrescalePanel _prescalePanel;
        private Transform _currentGhost;

        private static readonly FieldInfo GhostFieldInfo = typeof(AddPiece).GetField("_currentGhost",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private void Start()
        {
            Mod.OnToggle += (active) =>
            {
                if (!active) DestroyImmediate(_prescalePanel);
            };

            CheckForModUpdate();
        }

        private void Update()
        {
            if (!Mod.ModEnabled) return;

            // Handle key presses.
            if (BlockMapper.CurrentInstance != null)
            {
                if (_movingAllSliders && !Keybindings.Get(Mod.MoveAllSliderBinding).IsDown())
                    BlockMapper.CurrentInstance.Refresh();

                _movingAllSliders = Keybindings.Get(Mod.MoveAllSliderBinding).IsDown();

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
                {
                    if (Input.GetKey(KeyCode.C))
                        Mod.Copy();
                    if (Input.GetKey(KeyCode.V))
                        Mod.Paste();
                }
            }

            // Check for AddPiece and create or destroy PrescalePanel
            if (Game.AddPiece)
            {
                if (_prescalePanel == null)
                {
                    _prescalePanel = gameObject.AddComponent<PrescalePanel>();
                    _prescalePanel.OnScaleChanged += SetPrescale;
                    _prescalePanel.OnToggle += EnablePrescale;
                }
            }
            else
            {
                if (_prescalePanel != null)
                {
                    DestroyImmediate(_prescalePanel);
                }
            }

            // Update PrescalePanel slider values
            if (!Game.AddPiece) return;
            if (_currentBlockType == Game.AddPiece.BlockType && _currentGhost != null) return;
            _currentBlockType = Game.AddPiece.BlockType;
            _currentGhost = GhostFieldInfo.GetValue(Game.AddPiece) as Transform;

            if (_currentGhost == null || !Mod.PrescaleEnabled || !Mod.ModEnabled) return;
            if (Mod.PrescaleDictionary.ContainsKey(_currentBlockType))
            {
                _prescalePanel.Scale = Mod.PrescaleDictionary[_currentBlockType];
                _prescalePanel.RefreshSliderStrings();
            }
            else
            {
                _prescalePanel.Scale = PrefabMaster.GetDefaultScale(_currentBlockType);
                _prescalePanel.RefreshSliderStrings();
            }
        }

        private void EnablePrescale(bool enable)
        {
            var scale = enable && Mod.PrescaleDictionary.ContainsKey(_currentBlockType)
                ? Mod.PrescaleDictionary[_currentBlockType]
                : PrefabMaster.GetDefaultScale(_currentBlockType);
            ScaleGhost(scale);
        }

        private void SetPrescale(Vector3 scale)
        {
            if (Mod.PrescaleEnabled) ScaleGhost(scale);
            Mod.PrescaleDictionary[_currentBlockType] = scale;
        }

        private void ScaleGhost(Vector3 scale)
        {
            if (_currentBlockType == (int) BlockType.Wheel)
                scale.Scale(new Vector3(0.55f, 0.55f, 0.55f));
            if (_currentGhost)
                _currentGhost.localScale = scale;
        }

        private void CheckForModUpdate(bool verbose = false)
        {
            var updater = gameObject.AddComponent<Updater>();
            updater.Check(
                "Easy Scale Mod",
                "https://api.github.com/repos/lench4991/EasyScaleMod/releases/latest",
                Assembly.GetExecutingAssembly().GetName().Version,
                new List<Updater.Link>()
                    {
                            new Updater.Link() { DisplayName = "Spiderling forum page", URL = "http://forum.spiderlinggames.co.uk/index.php?threads/3314/" },
                            new Updater.Link() { DisplayName = "GitHub release page", URL = "https://github.com/lench4991/EasyScaleMod/releases/latest" }
                    },
                verbose);
        }
    }
}