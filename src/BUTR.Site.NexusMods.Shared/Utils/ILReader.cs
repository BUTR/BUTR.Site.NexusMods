using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace BUTR.Site.NexusMods.Shared.Utils
{
    public record ILInstruction(int Offset, ILOpCode OpCode, OperandType OperandType, int OperandOffset);

    public static class ILReader
    {
        private static readonly OperandType[] OperandTypes = Enumerable.Repeat((OperandType) 0xff, 0x11f).ToArray();

        static ILReader()
        {
            foreach (var field in typeof(OpCodes).GetFields())
            {
                var opCode = (OpCode) field.GetValue(null)!;
                var index = (ushort) (((opCode.Value & 0x200) >> 1) | opCode.Value & 0xff);

                OperandTypes[index] = opCode.OperandType;
            }
        }

        public static IEnumerable<ILInstruction> GetInstructions(MetadataReader reader, MethodBodyBlock methodBody)
        {
            var blob = methodBody.GetILReader();
            while (blob.RemainingBytes > 0)
            {
                var offset = blob.Offset;

                var opCode = ReadOpCode(ref blob);
                var operandType = GetOperandType(opCode);
                var operandOffset = blob.Offset;
                SkipOperand(ref blob, operandType);

                yield return new ILInstruction(offset, opCode, operandType, operandOffset);
            }
        }

        private static ILOpCode ReadOpCode(ref BlobReader blob)
        {
            var opCodeByte = blob.ReadByte();
            return (ILOpCode) (opCodeByte == 0xfe ? 0xfe00 + blob.ReadByte() : opCodeByte);
        }

        private static OperandType GetOperandType(ILOpCode opCode)
        {
            var index = (ushort) ((((int) opCode & 0x200) >> 1) | ((int) opCode & 0xff));
            return index >= OperandTypes.Length ? (OperandType) 0xff : OperandTypes[index];
        }

        private static void SkipOperand(ref BlobReader blob, OperandType operandType)
        {
            switch (operandType)
            {
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    blob.Offset += 8;
                    break;

                case OperandType.InlineBrTarget:
                case OperandType.InlineI:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    blob.Offset += 4;
                    break;

                case OperandType.InlineField:
                case OperandType.InlineMethod:
                    blob.Offset += 4;
                    break;

                case OperandType.InlineSwitch:
                    var length = blob.ReadInt32();
                    blob.Offset += length * 4;
                    break;

                case OperandType.InlineVar:
                    blob.Offset += 2;
                    break;

                case OperandType.ShortInlineVar:
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                    blob.Offset++;
                    break;
            }
        }
    }
}