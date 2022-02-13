using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ImGuiNET;
using Dalamud;
using Dalamud.IoC;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Logging;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;

using Poser.UI;

namespace Poser {
	public class Poser : IDalamudPlugin {
		private class Noper {
			private IntPtr address;
			private byte[] def;
			private byte[] nop;
			
			public Noper(string scan, int size) {
				this.address = SigScanner.ScanText(scan);
				SafeMemory.ReadBytes(address, size, out def);
				
				nop = new byte[size];
				for(int i = 0; i < size; i++)
					nop[i] = 0x90;
			}
			
			public void Nope() {
				SafeMemory.WriteBytes(address, nop);
			}
			
			public void Yup() {
				SafeMemory.WriteBytes(address, def);
			}
			
			public void Set(bool donop) {
				if(donop)
					Nope();
				else
					Yup();
			}
		}
		
		public string Name => "Poser";
		private const string command = "/poser";
		
		[PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface Interface   {get; private set;} = null!;
		[PluginService][RequiredVersion("1.0")] public static CommandManager         Commands    {get; private set;} = null!;
		[PluginService][RequiredVersion("1.0")] public static SigScanner             SigScanner  {get; private set;} = null!;
		[PluginService][RequiredVersion("1.0")] public static ClientState            ClientState {get; private set;} = null!;
		[PluginService][RequiredVersion("1.0")] public static GameGui                GameGui     {get; private set;} = null!;
		[PluginService][RequiredVersion("1.0")] public static TargetManager          Targets     {get; private set;} = null!;
		[PluginService][RequiredVersion("1.0")] public static ObjectTable            Objects     {get; private set;} = null!;
		
		private static GUI GUI;
		
		private static Noper nopPos1;
		private static Noper nopPos2;
		private static Noper nopPosPhys;
		private static Noper nopRot1;
		private static Noper nopRot2;
		private static Noper nopRot3;
		private static Noper nopRotPhys;
		private static Noper nopScale1;
		private static Noper nopScale2;
		private static Noper nopScalePhys;
		
		public Poser(DalamudPluginInterface pluginInterface) {
			GUI = new GUI();
			
			// TODO: fix many things disappearing when unfreezing physics
			nopPos1 = new("41 0F 29 24 12", 5);
			nopPos2 = new("43 0F 29 24 18", 5);
			nopPosPhys = new("0F 29 00 41 0F 28", 3);
			nopRot1 = new("41 0F 29 5C 12 10", 6);
			nopRot2 = new("43 0F 29 5C 18 10", 6);
			nopRot3 = new("0F 29 5E 10", 4);
			nopRotPhys = new("0F 29 48 10 41 0F 28", 4);
			nopScale1 = new("43 0F 29 44 18 20", 6);
			nopScale2 = new("41 0F 29 44 12 20", 6);
			nopScalePhys = new("0F 29 40 20 48 8B 46", 4);
			
			Poser.Commands.AddHandler(command, new CommandInfo(OnCommand) {
				HelpMessage = "Poser"
			});
		}
		
		public void Dispose() {
			FreezePosition(false);
			FreezeRotation(false);
			FreezeScale(false);
			FreezePhysics(false);
			
			GUI.Dispose();
			Poser.Commands.RemoveHandler(command);
		}
		
		public static void FreezePosition(bool freeze) {
			nopPos1.Set(freeze);
			nopPos2.Set(freeze);
		}
		
		public static void FreezeRotation(bool freeze) {
			nopRot1.Set(freeze);
			nopRot2.Set(freeze);
			nopRot3.Set(freeze);
		}
		
		public static void FreezeScale(bool freeze) {
			nopScale1.Set(freeze);
			nopScale2.Set(freeze);
		}
		
		public static void FreezePhysics(bool freeze) {
			nopPosPhys.Set(freeze);
			nopRotPhys.Set(freeze);
			nopScalePhys.Set(freeze);
		}
		
		private void OnCommand(string cmd, string args) {
			if(cmd == command)
				GUI.Show();
		}
	}
}