using System;
using System.Collections.Generic;
using Cloudpunk_Trainer.Models;
using Process_Memory;

namespace Cloudpunk_Trainer.Cloudpunk
{
	public sealed class PlayerCar : Hack
	{				
		Memory memory;
		MemoryCave memoryCave;

		IntPtr patchAddress = IntPtr.Zero;

		public override List<Offset> Offsets { get; } = new List<Offset>()
		{
			new Offset("id", typeof(int), 0x18),
			new Offset("maxStatus", typeof(int), 0x48),
			new Offset("maxFuel", typeof(int), 0x4C),
			new Offset("stabilizerConstant", typeof(float), 0x78),
			new Offset("stabilizerDamperConstant", typeof(float), 0x7C),
			new Offset("acceleration", typeof(float), 0x88),
			new Offset("minMotorTorque", typeof(float), 0x8C),
			new Offset("motorTorque", typeof(float), 0x90),
			new Offset("midMotorTorque", typeof(float), 0x94),
			new Offset("speedwayMotorTorque", typeof(float), 0x98),
			new Offset("minSteerTorque", typeof(float), 0x9C),
			new Offset("steerTorque", typeof(float), 0xA0),
			new Offset("midSteerTorque", typeof(float), 0xA4),
			new Offset("speedwaySteerTorque", typeof(float), 0xA8),
			new Offset("minMaximumSpeed", typeof(float), 0xAC),
			new Offset("maximumSpeed", typeof(float), 0xB0),
			new Offset("midMaximumSpeed", typeof(float), 0xB4),
			new Offset("maximumSpeedHighway", typeof(float), 0xB8),
			new Offset("currentMaxSpeed", typeof(float), 0xBC),
			new Offset("currentMotorTorque", typeof(float), 0xC0),
			new Offset("currentSteerTorque", typeof(float), 0xC4),
			new Offset("currentSpeed", typeof(float), 0xC8),
			new Offset("stableHeight", typeof(float), 0xCC),
			new Offset("maximumAngularVelocity", typeof(float), 0xD0),
			new Offset("stability", typeof(float), 0xD4),
			new Offset("reflection", typeof(float), 0xD8),
			new Offset("speedBoostTimer", typeof(float), 0x188),
			new Offset("currentFloorLevel", typeof(float), 0x1F0),
			new Offset("currentStatus", typeof(float), 0x2A8),
			new Offset("currentFuel", typeof(float), 0x2B0),
			new Offset("layerMinY", typeof(float), 0x2DC),
			new Offset("layerMaxY", typeof(float), 0x2E0),
			new Offset("verticalSpeed", typeof(float), 0x2E4),
			new Offset("nextFuelTaskCheck", typeof(float), 0x320),
		};

		public PlayerCar(Memory memory)
		{
			this.memory = memory;
		}

		public override bool Enabled
		{
			get;
			protected set;
		}

		public override long Value
		{
			get
			{
				return memory.Reader.ReadInt64(memoryCave.Index);
			}
		}

		public override bool Enable()
		{
			if (patchAddress == IntPtr.Zero)
			{
				patchAddress = memory.Pattern.Search("0F 2F 83 B0 02 00 00 0F", "GameAssembly.dll");
			}

			memoryCave = new MemoryCave(memory, patchAddress);

			// new code
			// mov [playercarstruct],rbx
			memoryCave.WriteBytes(new byte[] { 0x48, 0x89, 0x1D, 0x0C, 0x00, 0x00, 0x00 });
			// original code
			// comiss xmm0,[rbx+000002B0]
			memoryCave.WriteBytes(new byte[] { 0x0F, 0x2F, 0x83, 0xB0, 0x02, 0x00, 0x00 });
			// exit
			memoryCave.WriteJump(patchAddress + 5 + 2);
			// patch
			memoryCave.WriteJump(patchAddress, memoryCave.StartAddress, 2);

			Enabled = true;

			return true;
		}

		public override bool Disable()
		{
			memory.Writer.WriteBytes(patchAddress, new byte[] { 0x0F, 0x2F, 0x83, 0xB0, 0x02, 0x00, 0x00 });

			Enabled = false;

			return true;
		}
	}
}
