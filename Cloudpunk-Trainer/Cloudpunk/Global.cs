using System;
using System.Collections.Generic;
using Pro_Tweaker;
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

        public long Player
        {
            get
            {
                if (memoryCave != null)
                {
                    return memory.Reader.ReadInt64(new IntPtr(Value + 0x60));
                }
                else
                {
                    return -1;
                }
            }
        }

        public long PlayerCar
        {
            get
            {
                if (memoryCave != null)
                {
                    return memory.Reader.ReadInt64(new IntPtr(Value + 0x70));
                }
                else
                {
                    return -1;
                }
            }
        }
        
        public Global(Memory memory)
		{
			this.memory = memory;
		}

		public override bool Enable()
		{	
			if(patchAddress == IntPtr.Zero)
			{
				patchAddress = memory.Pattern.Search("F3 0F 10 81 C4 00 00 00 C3 CC CC CC CC CC CC CC F3", "GameAssembly.dll");
			}
			
			memoryCave = new MemoryCave(memory, patchAddress);

			// new code
			// mov [globalstruct],rcx
			memoryCave.WriteBytes(new byte[] { 0x48, 0x89, 0x0D, 0x0D, 0x00, 0x00, 0x00 });
			// original code
			// movss xmm0,[rcx+000000C4]
			memoryCave.WriteBytes(new byte[] { 0xF3, 0x0F, 0x10, 0x81, 0xC4, 0x00, 0x00, 0x00 });
			 // exit
			memoryCave.WriteJump(patchAddress + 5 + 3);
			// patch
			memoryCave.WriteJump(patchAddress, memoryCave.StartAddress, 3);

			Enabled = true;

			return true;
		}

		public override bool Disable()
		{
			bool success = memory.Writer.WriteBytes(patchAddress, new byte[] { 0xF3, 0x0F, 0x10, 0x81, 0xC4, 0x00, 0x00, 0x00 });

			Enabled = false;

			return success;
		}
	}
}
