using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Dalamud.Logging;
using Poser.Memory;
using System.Threading;

namespace Poser {
	public unsafe class Bone {
		public Vector3 Position {get; private set;}
		public Quaternion Rotation {get; private set;}
		public Vector3 Scale {get; private set;}
		public Transform Transform;
		
		public string Name {get; private set;}
		public List<Bone> Children {get; private set;}
		public Bone? Parent {get; private set;}
		public Bone[] Bones {get; private set;}
		
		private IntPtr address;
		private Transform root;
		
		public Bone(IntPtr address, Transform root, string? name) {
			Name = name ?? "unnamed";
			Children = new();
			
			this.address = address;
			Transform = Marshal.PtrToStructure<Transform>(address);
			this.root = root;
		}
		
		private void ParentTo(Bone parent) {
			Parent = parent;
			Parent.Children.Add(this);
		}
		
		public static Bone CreateSkeleton(HkaPose pose, Transform root) {
			var hkaSkel = *pose.Skeleton;
			var boneCount = pose.TransformCount;
			Bone[] bones = new Bone[boneCount];
			
			// Create all bones
			for(int boneI = 0; boneI < boneCount; boneI++) {
				var name = Marshal.PtrToStringUTF8(Marshal.PtrToStructure<IntPtr>(hkaSkel.Names + boneI * 16));
				bones[boneI] = new Bone(pose.Transforms + boneI * 0x30, root, name);
				bones[boneI].Bones = bones;
			}
			
			// Figure out the parent tree
			int rootI = 0;
			for(int boneI = 0; boneI < boneCount; boneI++) {
				var parentI = Marshal.PtrToStructure<short>(hkaSkel.Parents + boneI * 2);
				
				if(parentI < 0)
					rootI = boneI;
				else
					bones[boneI].ParentTo(bones[parentI]);
			}
			
			bones[rootI].CalculateTransform();
			return bones[rootI];
		}
		
		public void CalculateTransform() {
			// Position = Transform.Position;
			// Rotation = Transform.Rotation;
			// Scale = Transform.Scale;
			
			Position = Vector3.Transform((Vector3)Transform.Position * root.Scale, root.Rotation) + root.Position;
			Rotation = root.Rotation * (Quaternion)Transform.Rotation;
			Scale = (Vector3)Transform.Scale * root.Scale;
			
			foreach(Bone child in Children)
				child.CalculateTransform();
		}
		
		public Bone FindBoneByName(string name) {
			if(Name == name)
				return this;
			
			foreach(var child in Children) {
				var r = child.FindBoneByName(name);
				if(r != null)
					return r;
			}
			
			return null;
		}
		
		public void Apply() {
			Marshal.StructureToPtr(Transform, address, true);
			
			foreach(Bone child in Children)
				child.Apply();
		}
		
		public void MoveBy(Vector3 pos, bool relative = true, bool inherit = true) {
			if(Parent == null)
				relative = false;
			
			var p = (Vector3)Transform.Position;
			
			if(relative)
				p = Vector3.Transform(p - Parent.Transform.Position, Parent.Transform.Rotation);
			
			p.X += pos.X;
			p.Y += pos.Y;
			p.Z += pos.Z;
			
			if(relative)
				p = Vector3.Transform(p, Quaternion.Inverse(Parent.Transform.Rotation)) + Parent.Transform.Position;
			
			Transform.Position.X = p.X;
			Transform.Position.Y = p.Y;
			Transform.Position.Z = p.Z;
			
			if(inherit)
				foreach(Bone child in Children)
					child.MoveBy(relative ? Vector3.Transform(pos, Quaternion.Inverse(Parent.Transform.Rotation)) : pos, false, inherit);
		}
		
		public void SetPosition(Vector3 pos, bool inherit = true) {
			MoveBy(pos - Transform.Position, false, inherit);
		}
		
		public void RotateBy(Quaternion quat, bool inherit = true, Bone origin = null) {
			var q = quat * Transform.Rotation;
			Transform.Rotation.X = q.X;
			Transform.Rotation.Y = q.Y;
			Transform.Rotation.Z = q.Z;
			Transform.Rotation.W = q.W;
			
			if(origin != null) {
				var p = origin.Transform.Position + Vector3.Transform((Vector3)Transform.Position - origin.Transform.Position, quat);
				Transform.Position.X = p.X;
				Transform.Position.Y = p.Y;
				Transform.Position.Z = p.Z;
			}
			
			if(inherit)
				foreach(Bone child in Children)
					child.RotateBy(quat, inherit, origin ?? this);
		}
		
		public void SetRotation(Quaternion quat, bool inherit = true) {
			RotateBy(quat * Quaternion.Inverse(Transform.Rotation), inherit);
		}
		
		public void ScaleBy(Vector3 scale, bool inherit = true) {
			Transform.Scale.X *= scale.X;
			Transform.Scale.Y *= scale.Y;
			Transform.Scale.Z *= scale.Z;
			
			if(inherit)
				foreach(Bone child in Children)
					child.ScaleBy(scale, inherit);
		}
		
		public void SetScale(Vector3 scale, bool inherit = true) {
			ScaleBy(scale - Transform.Scale + Vector3.One, inherit);
		}
	}
}