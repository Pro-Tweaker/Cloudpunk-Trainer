using System;
using System.Collections.Generic;
using Process_Memory;
using Cloudpunk_Trainer.Models;

namespace Cloudpunk_Trainer.Cloudpunk
{
	public sealed class Global : Hack
	{		
		Memory memory;
		MemoryCave memoryCave;

		IntPtr patchAddress = IntPtr.Zero;
			   
		public override bool Enabled
		{
			get;
			protected set;
		}

		public override long Value
		{
			get
			{
				if(memoryCave != null)
				{
					return memory.Reader.ReadInt64(memoryCave.Index);
				}
				else
				{
					return -1;
				}
			}
		}

		public override List<Offset> Offsets { get; } = new List<Offset>()
        {
			new Offset("idleMusicTimer", typeof(float), 0xB8),
			new Offset("advertisementTimer", typeof(float), 0xBC),
			new Offset("makeCallTimer", typeof(float), 0x49C),
			new Offset("money", typeof(int), 0x4D0),
			new Offset("numLocationsUnlocked", typeof(int), 0x560),
			new Offset("numRepairs", typeof(int), 0x564),
			new Offset("joosTimePassed", typeof(float), 0x5D0),
			new Offset("joosTimeLeft", typeof(float), 0x5D4),
			new Offset("alcoholTimePassed", typeof(float), 0x600),
			new Offset("alcoholTimeLeft", typeof(float), 0x604),
			new Offset("speedGainTimeLeft", typeof(float), 0xB8),
			new Offset("stimsTimePassed", typeof(float), 0x608),
			new Offset("stimsTimeLeft", typeof(float), 0x610),
			new Offset("pheromonesTimePassed", typeof(float), 0x614),
			new Offset("pheromonesTimeLeft", typeof(float), 0x618),
			new Offset("foodCooldown", typeof(float), 0x61C),
			new Offset("drinkCooldown", typeof(float), 0x620),
			new Offset("drugCooldown", typeof(float), 0x624),
			new Offset("timeSinceStart", typeof(float), 0x6C8)
		};

		public Global(Memory memory)
		{
			this.memory = memory;
		}

		public override bool Enable()
		{	
			if(patchAddress == IntPtr.Zero)
			{
				patchAddress = memory.Pattern.Search("F3 0F 10 81 BC 00 00 00 C3 CC CC CC CC CC CC CC F3", "GameAssembly.dll");
			}
			
			memoryCave = new MemoryCave(memory, patchAddress);

			// new code
			// mov [globalstruct],rcx
			memoryCave.WriteBytes(new byte[] { 0x48, 0x89, 0x0D, 0x0D, 0x00, 0x00, 0x00 });
			// original code
			// movss xmm0,[rcx+000000BC]
			memoryCave.WriteBytes(new byte[] { 0xF3, 0x0F, 0x10, 0x81, 0xBC, 0x00, 0x00, 0x00 });
			 // exit
			memoryCave.WriteJump(patchAddress + 5 + 3);
			// patch
			memoryCave.WriteJump(patchAddress, memoryCave.StartAddress, 3);

			Enabled = true;

			return true;
		}

		public override bool Disable()
		{
			memory.Writer.WriteBytes(patchAddress, new byte[] { 0xF3, 0x0F, 0x10, 0x81, 0xBC, 0x00, 0x00, 0x00 });

			Enabled = false;

			return true;
		}
	}
}
