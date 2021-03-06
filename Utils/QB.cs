using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MHPB2_LevelTool
{
    public class QB
    {
        public static SortedList<uint, string> gChecksumTable;
        public static uint[] CRCTable =
        {
                0x00000000, 0x77073096, 0xee0e612c, 0x990951ba,
                0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
                0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
                0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
                0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
                0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
                0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec,
                0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
                0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
                0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
                0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940,
                0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
                0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116,
                0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
                0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
                0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
                0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a,
                0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
                0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818,
                0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
                0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
                0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
                0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c,
                0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
                0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
                0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
                0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
                0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
                0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086,
                0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
                0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4,
                0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
                0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
                0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
                0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
                0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
                0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe,
                0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
                0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
                0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
                0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252,
                0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
                0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60,
                0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
                0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
                0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
                0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04,
                0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
                0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a,
                0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
                0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
                0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
                0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e,
                0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
                0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
                0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
                0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
                0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
                0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0,
                0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
                0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6,
                0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
                0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
                0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
        };

        public static uint GenerateCRC(string name)
        {
            if (null == gChecksumTable)
                gChecksumTable = new SortedList<uint, string>();

            if (name == string.Empty)
                return 0;

            name = name.ToLower();

            uint checksum = 0xffffffff;

            for (int i = 0; i < name.Length; i++)
            {
                char ch = name[i];
                checksum = CRCTable[(checksum ^ ch) & 0xff] ^ ((checksum >> 8) & 0x00ffffff);
            }

            if (!gChecksumTable.ContainsKey(checksum))
                gChecksumTable.Add(checksum, name);

            return checksum;
        }


        public enum EScriptToken
        {
            // Misc
            ESCRIPTTOKEN_ENDOFFILE,			// 0
            ESCRIPTTOKEN_ENDOFLINE,			// 1
            ESCRIPTTOKEN_ENDOFLINENUMBER,   // 2
            ESCRIPTTOKEN_STARTSTRUCT,       // 3
            ESCRIPTTOKEN_ENDSTRUCT,         // 4
            ESCRIPTTOKEN_STARTARRAY,        // 5
            ESCRIPTTOKEN_ENDARRAY,          // 6
            ESCRIPTTOKEN_EQUALS,            // 7
            ESCRIPTTOKEN_DOT,               // 8
            ESCRIPTTOKEN_COMMA,             // 9
            ESCRIPTTOKEN_MINUS,             // 10
            ESCRIPTTOKEN_ADD,               // 11
            ESCRIPTTOKEN_DIVIDE,            // 12
            ESCRIPTTOKEN_MULTIPLY,          // 13
            ESCRIPTTOKEN_OPENPARENTH,       // 14
            ESCRIPTTOKEN_CLOSEPARENTH,      // 15

            // This is ignored by the interpreter.
            // Allows inclusion of source level debugging info, eg line number.
            ESCRIPTTOKEN_DEBUGINFO,			// 16

            // Comparisons
            ESCRIPTTOKEN_SAMEAS,			// 17
            ESCRIPTTOKEN_LESSTHAN,			// 18
            ESCRIPTTOKEN_LESSTHANEQUAL,     // 19
            ESCRIPTTOKEN_GREATERTHAN,       // 20
            ESCRIPTTOKEN_GREATERTHANEQUAL,  // 21

            // Types
            ESCRIPTTOKEN_NAME,				// 22
            ESCRIPTTOKEN_INTEGER,			// 23
            ESCRIPTTOKEN_HEXINTEGER,        // 24
            ESCRIPTTOKEN_ENUM,              // 25
            ESCRIPTTOKEN_FLOAT,             // 26
            ESCRIPTTOKEN_STRING,            // 27
            ESCRIPTTOKEN_LOCALSTRING,       // 28
            ESCRIPTTOKEN_ARRAY,             // 29
            ESCRIPTTOKEN_VECTOR,            // 30
            ESCRIPTTOKEN_PAIR,				// 31

            // Key words
            ESCRIPTTOKEN_KEYWORD_BEGIN,		// 32
            ESCRIPTTOKEN_KEYWORD_REPEAT,    // 33
            ESCRIPTTOKEN_KEYWORD_BREAK,     // 34
            ESCRIPTTOKEN_KEYWORD_SCRIPT,    // 35
            ESCRIPTTOKEN_KEYWORD_ENDSCRIPT, // 36
            ESCRIPTTOKEN_KEYWORD_IF,        // 37
            ESCRIPTTOKEN_KEYWORD_ELSE,      // 38
            ESCRIPTTOKEN_KEYWORD_ELSEIF,    // 39
            ESCRIPTTOKEN_KEYWORD_ENDIF,		// 40
            ESCRIPTTOKEN_KEYWORD_RETURN,	// 41

            ESCRIPTTOKEN_UNDEFINED,			// 42

            // For debugging					  
            ESCRIPTTOKEN_CHECKSUM_NAME,		// 43

            // Token for the <...> symbol					
            ESCRIPTTOKEN_KEYWORD_ALLARGS,	// 44
            // Token that preceds a name when the name is enclosed in < > in the source.
            ESCRIPTTOKEN_ARG,				// 45

            // A relative jump. Used to speed up if-else-endif and break statements, and
            // used to jump to the end of lists of items in the random operator.
            ESCRIPTTOKEN_JUMP,				// 46
            // Precedes a list of items that are to be randomly chosen from.
            ESCRIPTTOKEN_KEYWORD_RANDOM,    // 47

            // Precedes two integers enclosed in parentheses.
            ESCRIPTTOKEN_KEYWORD_RANDOM_RANGE,	// 48

            // Only used internally by qcomp, never appears in a .qb
            ESCRIPTTOKEN_AT,				// 49

            // Logical operators
            ESCRIPTTOKEN_OR,				// 50
            ESCRIPTTOKEN_AND,				// 51
            ESCRIPTTOKEN_XOR,				// 52

            // Shift operators
            ESCRIPTTOKEN_SHIFT_LEFT,		// 53
            ESCRIPTTOKEN_SHIFT_RIGHT,		// 54

            // These versions use the Rnd2 function, for use in certain things so as not to mess up
            // the determinism of the regular Rnd function in replays.
            ESCRIPTTOKEN_KEYWORD_RANDOM2,		// 55
            ESCRIPTTOKEN_KEYWORD_RANDOM_RANGE2, // 56

            ESCRIPTTOKEN_KEYWORD_NOT,			// 57
            ESCRIPTTOKEN_KEYWORD_AND,			// 58
            ESCRIPTTOKEN_KEYWORD_OR,            // 59
            ESCRIPTTOKEN_KEYWORD_SWITCH,       	// 60
            ESCRIPTTOKEN_KEYWORD_ENDSWITCH,   	// 61
            ESCRIPTTOKEN_KEYWORD_CASE,          // 62
            ESCRIPTTOKEN_KEYWORD_DEFAULT,		// 63

            ESCRIPTTOKEN_KEYWORD_RANDOM_NO_REPEAT,	// 64
            ESCRIPTTOKEN_KEYWORD_RANDOM_PERMUTE,	// 65

            ESCRIPTTOKEN_COLON,		// 66

            // These are calculated at runtime in the game code by PreProcessScripts,
            // so they never appear in a qb file.
            ESCRIPTTOKEN_RUNTIME_CFUNCTION,	// 67
            ESCRIPTTOKEN_RUNTIME_MEMBERFUNCTION, // 68

            // Warning! Do not exceed 256 entries, since these are stored in bytes.
        };

        public static void qb_write_checksum(BinaryWriter writer, string a, string b, bool newline = true)
        {
            if (newline)
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

            if (a != String.Empty)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(a));
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
            }

            if (b != String.Empty)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(b));
            }

        }

        public static void qb_write_int(BinaryWriter writer, string a, int b, bool newline = true)
        {
            if (newline)
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

            if (a != String.Empty)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(a));
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
            }

            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_INTEGER);
            writer.Write(b);
        }

        public static void qb_write_float(BinaryWriter writer, string a, float b, bool newline = true)
        {
            if (newline)
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

            if (a != String.Empty)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(a));
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
            }

            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_FLOAT);
            writer.Write(b);
        }

        public static void qb_write_vector(BinaryWriter writer, string a, Vec3 b, bool newline = true)
        {
            if (newline)
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

            if (a != String.Empty)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(a));
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
            }

            if (!b.isNull())
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_VECTOR);
                writer.Write(b.x);
                writer.Write(b.y);
                writer.Write(b.z);
            }
        }

        public static void qb_write_single_string(BinaryWriter writer, string a)
        {
            char[] stringBytes = a.ToArray();
            foreach (char c in stringBytes)
            {
                if (c != 0x0e)
                {
                    writer.Write((byte)c);
                }
            }
            writer.Write((byte)0);
        }

        public static void qb_write_string(BinaryWriter writer, string a, string b, bool newline = true)
        {
            if (newline)
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

            if (a != String.Empty)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(a));
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
            }

            if (b != null)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_STRING);
                writer.Write(b.Length + 1);
                qb_write_single_string(writer, b);
            }
        }

        public enum eQBItem
        {
            CHECKSUM,
            STRING,
            FLOAT,
            INTEGER,
            VECTOR2,
            VECTOR3
        };

        public class QBScript
        {
            public string name;
            public List<QBInstruction> instructions;

            public QBScript()
            {
                name = String.Empty;
                instructions = new List<QBInstruction>();
            }

            public void dump(BinaryWriter writer)
            {
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_KEYWORD_SCRIPT);
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                writer.Write(QB.GenerateCRC(name));

                foreach (QB.QBInstruction instruction in instructions)
                {
                    if (instruction.functionCallName != String.Empty)
                    {
                        qb_write_function_call(writer, instruction.functionCallName, instruction.args, true);
                    }
                    else
                    {
                        foreach (QB.QBItem arg in instruction.args)
                        {
                            arg.dump(writer);
                        }
                    }
                }


                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_KEYWORD_ENDSCRIPT);
            }
        }

        public class QBInstruction
        {
            public string functionCallName;
            public List<QBItem> args;

            public QBInstruction()
            {
                functionCallName = String.Empty;
                args = new List<QBItem>();
            }
        }

        public class QBItem
        {
            public bool newline;
            public string name;
            public string value_checksum;
            public string value_string;
            public int value_int;
            public float value_float;
            public float value_x;
            public float value_y;
            public float value_z;
            public eQBItem type;

            public QBItem()
            {
                name = String.Empty;
                value_checksum = String.Empty;
                value_string = String.Empty;
                newline = false;
            }

            public QBItem(eQBItem _type, string _name, string _value, bool _newline = false)
            {
                name = String.Empty;
                value_checksum = String.Empty;
                value_string = String.Empty;

                type = _type;
                name = _name;
                newline = _newline;

                if (_type == eQBItem.STRING)
                {
                    value_string = _value;
                }
                else if (_type == eQBItem.CHECKSUM)
                {
                    value_checksum = _value;
                }
            }

            public QBItem(eQBItem _type, string _name, int _value, bool _newline = false)
            {
                name = String.Empty;
                value_checksum = String.Empty;
                value_string = String.Empty;

                type = _type;
                name = _name;
                newline = _newline;
                value_int = _value;
            }

            public QBItem(eQBItem _type, string _name, float _value, bool _newline = false)
            {
                name = String.Empty;
                value_checksum = String.Empty;
                value_string = String.Empty;

                type = _type;
                name = _name;
                newline = _newline;
                value_float = _value;
            }

            public void dump(BinaryWriter writer)
            {
                if (newline)
                    writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

                switch (type)
                {
                    case eQBItem.CHECKSUM:
                        if (name != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(name));
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
                        }
                        if (value_checksum != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(value_checksum));
                        }
                        break;
                    case eQBItem.STRING:
                        if (name != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(name));
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
                        }
                        writer.Write((byte)EScriptToken.ESCRIPTTOKEN_STRING);
                        writer.Write(value_string.Length + 1);
                        qb_write_single_string(writer, value_string);
                        break;
                    case eQBItem.FLOAT:
                        if (name != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(name));
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
                        }
                        writer.Write((byte)EScriptToken.ESCRIPTTOKEN_FLOAT);
                        writer.Write(value_float);
                        break;
                    case eQBItem.INTEGER:
                        if (name != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(name));
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
                        }
                        writer.Write((byte)EScriptToken.ESCRIPTTOKEN_INTEGER);
                        writer.Write(value_int);
                        break;
                    case eQBItem.VECTOR2:
                        if (name != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(name));
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
                        }
                        writer.Write((byte)EScriptToken.ESCRIPTTOKEN_PAIR);
                        writer.Write(value_x);
                        writer.Write(value_y);
                        break;
                    case eQBItem.VECTOR3:
                        if (name != String.Empty)
                        {
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
                            writer.Write(QB.GenerateCRC(name));
                            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_EQUALS);
                        }
                        writer.Write((byte)EScriptToken.ESCRIPTTOKEN_VECTOR);
                        writer.Write(value_x);
                        writer.Write(value_y);
                        writer.Write(value_z);
                        break;
                };

            }
        }

        public static void qb_write_function_call(BinaryWriter writer, string function, List<QBItem> args, bool newline = true)
        {
            if (newline)
                writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDOFLINE);

            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_NAME);
            writer.Write(QB.GenerateCRC(function));

            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_STARTSTRUCT);

            foreach(QB.QBItem arg in args)
            {
                arg.dump(writer);
            }

            writer.Write((byte)EScriptToken.ESCRIPTTOKEN_ENDSTRUCT);

        }
    }
}
