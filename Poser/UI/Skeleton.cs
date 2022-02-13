using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ImGuiNET;
using Dalamud.Logging;

using Poser.Memory;

namespace Poser.UI {
	internal partial class GUI {
		private const float childlessLength = 0.05f;
		
		private void DrawSkeleton(Bone skeleton) {
			Dictionary<Bone, Vector2> points = new();
			foreach(Bone bone in skeleton.Bones) {
				Poser.GameGui.WorldToScreen(bone.Position, out var p);
				points[bone] = p;
			}
			
			void drawBone(Bone bone) {
				string name = bone.Name;
				float thickness = (boneHover == name ? 5f : (boneSelected == name ? 3f : 1f)) * bone.Scale.Length();
				var clr = boneHover == name ? colorBoneHover : (boneSelected == name ? colorBoneSelected : colorBone);
				
				ImGui.GetWindowDrawList().AddCircleFilled(points[bone], thickness + 1f, clr, 100);
				
				if(bone.Children.Count == 0) {
					Poser.GameGui.WorldToScreen(bone.Position + Vector3.Transform(new Vector3(childlessLength, 0, 0), bone.Rotation), out var pos2);
					ImGui.GetWindowDrawList().AddLine(points[bone], pos2, clr, thickness);
				} else {
					foreach(Bone child in bone.Children) {
						ImGui.GetWindowDrawList().AddLine(points[bone], points[child], clr, thickness);
						// Poser.GameGui.WorldToScreen(bone.Position + Vector3.Transform(new Vector3((bone.Position - child.Position).Length(), 0, 0), bone.Rotation), out var pos2);
						// ImGui.GetWindowDrawList().AddLine(points[bone], pos2, clr, thickness);
						
						drawBone(child);
					}
				}
			}
			
			drawBone(skeleton);
		}
	}
}