using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ImGuiNET;
using Dalamud.Logging;
using Dalamud.Interface;

using Poser.Memory;

namespace Poser.UI {
	internal partial class GUI : IDisposable {
		private const uint colorBone = 0xAABBBBBB;
		private const uint colorBoneHover = 0xFFFFFFFF;
		private const uint colorBoneSelected = 0xFFAA88CC;
		private const int gposeActorOffset = 0x98;
		
		public bool shouldDraw;
		private bool drawSkeleton = true;
		private bool freezePos = false;
		private bool freezeRot = false;
		private bool freezeScale = false;
		private bool freezePhys = false;
		
		private IntPtr lastActorAddress;
		private List<Bone> skeletons;
		private string? boneHover;
		private string? boneSelected;
		
		public GUI() {
			shouldDraw = true;
			skeletons = new();
			
			Poser.Interface.UiBuilder.DisableGposeUiHide = true;
			Poser.Interface.UiBuilder.OpenConfigUi += Show;
			Poser.Interface.UiBuilder.Draw += Draw;
		}
		
		public void Dispose() {
			Poser.Interface.UiBuilder.OpenConfigUi -= Show;
			Poser.Interface.UiBuilder.Draw -= Draw;
		}
		
		public void Show() {
			shouldDraw = !shouldDraw;
		}
		
		private void Draw() {
			if(!shouldDraw)
				return;
			
			unsafe {
				try {
					var actorAddress = Poser.Objects[201] != null ? Marshal.PtrToStructure<IntPtr>(Poser.Targets.Address + gposeActorOffset) : (Poser.Targets.Target?.Address ?? Poser.ClientState.LocalPlayer.Address);
					
					// if(lastActorAddress != actorAddress) {
						lastActorAddress = actorAddress;
						// PluginLog.Log("change target");
						var actor = Marshal.PtrToStructure<Actor>(actorAddress);
						var skelObj = *(*actor.DrawObject).Skeleton;
						
						skeletons = new();
						for(int i = 0; i < 1; i++) {
						// for(int i = 0; i < skelObj.SkeletonCount; i++) {
							var skel = Marshal.PtrToStructure<PartialSkeleton>(skelObj.Skeletons + i * 8);
							var pose = *skel.Pose;
							
							skeletons.Add(Bone.CreateSkeleton(pose, skelObj.Transform));
						}
					// }
				} catch {}
			}
			
			// World Overlay
			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
			ImGuiHelpers.ForceNextWindowMainViewport();
			ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));
			ImGui.Begin("PoserSkeleton",
				ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar |
				ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
			ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
			
			if(drawSkeleton)
				foreach(Bone skeleton in skeletons)
					DrawSkeleton(skeleton);
			// try {
			// 	DrawActor(skeletonObj);
			// } catch(Exception e) {PluginLog.LogError(e, "draw actor");}
			
			ImGui.End();
			ImGui.PopStyleVar();
			
			// Menu
			ImGui.SetNextWindowSize(new Vector2(450, 600), ImGuiCond.FirstUseEver);
			ImGui.Begin("Poser", ref shouldDraw);
			
			ImGui.Checkbox("Draw Skeleton", ref drawSkeleton);
			
			if(ImGui.Checkbox("Freeze Position", ref freezePos))
				Poser.FreezePosition(freezePos);
			
			if(ImGui.Checkbox("Freeze Rotation", ref freezeRot))
				Poser.FreezeRotation(freezeRot);
			
			if(ImGui.Checkbox("Freeze Scale", ref freezeScale))
				Poser.FreezeScale(freezeScale);
				
			if(ImGui.Checkbox("Freeze Physics", ref freezePhys))
				Poser.FreezePhysics(freezePhys);
			
			if(boneSelected != null && skeletons.Count > 0) {
				var bone = skeletons[0].FindBoneByName(boneSelected);
				if(bone != null) {
					if(ImGui.Button("Explode Bone (this will break stuff until zone reload)"))
						bone.SetScale(new Vector3(float.NaN));
					
					var pos = (Vector3)bone.Transform.Position;
					ImGui.SliderFloat("Position X", ref pos.X, -10, 10);
					ImGui.SliderFloat("Position Y", ref pos.Y, -10, 10);
					ImGui.SliderFloat("Position Z", ref pos.Z, -10, 10);
					bone.SetPosition(pos);
					
					ImGui.Separator();
					
					var ang = (Quaternion)bone.Transform.Rotation;
					ImGui.TextWrapped("Enjoy dealing with quaternions lol, gonna be impossible, cba making it eular for now");
					ImGui.SliderFloat("Rotation X", ref ang.X, -(float)Math.PI, (float)Math.PI);
					ImGui.SliderFloat("Rotation Y", ref ang.Y, -(float)Math.PI, (float)Math.PI);
					ImGui.SliderFloat("Rotation Z", ref ang.Z, -(float)Math.PI, (float)Math.PI);
					ImGui.SliderFloat("Rotation W", ref ang.W, -(float)Math.PI, (float)Math.PI);
					bone.SetRotation(ang);
					
					ImGui.Separator();
					
					var scale = (Vector3)bone.Transform.Scale;
					ImGui.SliderFloat("Scale X", ref scale.X, 0, 2);
					ImGui.SliderFloat("Scale Y", ref scale.Y, 0, 2);
					ImGui.SliderFloat("Scale Z", ref scale.Z, 0, 2);
					bone.SetScale(scale);
					
					bone.CalculateTransform();
					bone.Apply();
				}
			}
			
			foreach(Bone skeleton in skeletons)
				DrawBoneSelect(skeleton);
			
			ImGui.End();
		}
		
		private void DrawBoneSelect(Bone bone, int level = 0) {
			var selected = boneSelected == bone.Name;
			if(selected)
				ImGui.PushStyleColor(ImGuiCol.Text, colorBoneSelected);
			
			if(ImGui.Button(new string('\t', level) + bone.Name)) {
				// bone.ScaleBy(new Vector3(2, 2, 2), true);
				// bone.SetRotation(Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), (float)Math.PI / 2));
				// bone.RotateBy(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 20));
				// bone.MoveBy(new Vector3(0.1f, 0, 0));
				// bone.CalculateTransform();
				// bone.Apply();
				boneSelected = bone.Name;
			}
			
			if(selected)
				ImGui.PopStyleColor();
			
			foreach(Bone child in bone.Children)
				DrawBoneSelect(child, level + 1);
		}
	}
}