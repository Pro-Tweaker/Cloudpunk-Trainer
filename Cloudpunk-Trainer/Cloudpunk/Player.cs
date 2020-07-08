using Cloudpunk_Trainer.Models;
using Process_Memory;
using System;
using System.Collections.Generic;

namespace Cloudpunk_Trainer.Cloudpunk
{
	public sealed class Player : Hack
    {
		Memory memory;
		MemoryCave memoryCave;

		IntPtr patchAddress = IntPtr.Zero;

		public override List<Offset> Offsets { get; } = new List<Offset>()
		{
			new Offset("health", typeof(int), 0x40),
			new Offset("intendedZoomFactor", typeof(float), 0x128),
			new Offset("runSpeed", typeof(float), 0x190),
			new Offset("sneakSpeed", typeof(float), 0x194),
			new Offset("gravity", typeof(float), 0x198),
		};

		public Player(Memory memory)
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
				patchAddress = memory.Pattern.Search("88 83 07 02 00 00", "GameAssembly.dll");
			}

			memoryCave = new MemoryCave(memory, patchAddress);

			// new code
			// mov [playerstruct],rbx
			memoryCave.WriteBytes(new byte[] { 0x48, 0x89, 0x1D, 0x0B, 0x00, 0x00, 0x00 });
			// original code
			// mov [rbx+00000207],al
			memoryCave.WriteBytes(new byte[] { 0x88, 0x83, 0x07, 0x02, 0x00, 0x00 });
			// exit
			memoryCave.WriteJump(patchAddress + 5 + 1);
			// patch
			memoryCave.WriteJump(patchAddress, memoryCave.StartAddress, 1);

			Enabled = true;

			return true;
		}

		public override bool Disable()
		{
			memory.Writer.WriteBytes(patchAddress, new byte[] { 0x88, 0x83, 0x07, 0x02, 0x00, 0x00 });

			Enabled = false;

			return true;
		}
	}
}
