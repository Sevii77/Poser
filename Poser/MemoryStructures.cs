using System;
using System.Runtime.InteropServices;
using Num = System.Numerics;

namespace Poser.Memory {
	// https://github.com/imchillin/Anamnesis/tree/master/Anamnesis/Memory for a decent amount of the structures
	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct Actor {
		[FieldOffset(0x30)] public fixed byte Name[64];
		[FieldOffset(0x74)] public uint ObjectID;
		[FieldOffset(0xF0)] public DrawObject* DrawObject;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct DrawObject {
		[FieldOffset(0xA0)] public SkeletonObject* Skeleton;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct SkeletonObject {
		[FieldOffset(0x20)] public Transform Transform;
		[FieldOffset(0x50)] public ushort SkeletonCount;
		[FieldOffset(0x68)] public IntPtr Skeletons;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct PartialSkeleton {
		[FieldOffset(0x12C)] public short ConnectedBone;
		[FieldOffset(0x12E)] public short ConnectedParentBone;
		[FieldOffset(0x140)] public HkaPose* Pose;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct HkaPose {
		[FieldOffset(0x0)] public HkaSkeleton* Skeleton;
		[FieldOffset(0x10)] public int TransformCount;
		[FieldOffset(0x18)] public IntPtr Transforms;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct HkaSkeleton {
		[FieldOffset(0x18)] public IntPtr Parents;
		[FieldOffset(0x28)] public IntPtr Names;
	}
	
	[StructLayout(LayoutKind.Explicit, Size = 0x30)]
	public struct Transform {
		[FieldOffset(0x0)] public Vector3 Position;
		[FieldOffset(0x10)] public Quaternion Rotation;
		[FieldOffset(0x20)] public Vector3 Scale;
		
		[StructLayout(LayoutKind.Explicit, Size = 0x10)]
		public struct Quaternion {
			[FieldOffset(0x0)] public float X;
			[FieldOffset(0x4)] public float Y;
			[FieldOffset(0x8)] public float Z;
			[FieldOffset(0xC)] public float W;
			
			public static implicit operator Num.Quaternion(Quaternion quat) {
				return new Num.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
			}
			
			public static explicit operator Quaternion(Num.Quaternion quat) {
				return new Quaternion{X = quat.X, Y = quat.Y, Z = quat.Z, W = quat.W};
			}
		}
		
		[StructLayout(LayoutKind.Explicit, Size = 0x10)]
		public struct Vector3 {
			[FieldOffset(0x0)] public float X;
			[FieldOffset(0x4)] public float Y;
			[FieldOffset(0x8)] public float Z;
			[FieldOffset(0xC)] public float W;
			
			public static implicit operator Num.Vector3(Vector3 vec) {
				return new Num.Vector3(vec.X, vec.Y, vec.Z);
			}
			
			public static explicit operator Vector3(Num.Vector3 vec) {
				return new Vector3{X = vec.X, Y = vec.Y, Z = vec.Z};
			}
		}
	}
}