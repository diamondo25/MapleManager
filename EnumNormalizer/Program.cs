using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumNormalizer
{
    class Program
    {




        enum CT
        {
            CT_TEXT = 0x0,
            CT_ICON = 0x1,
            CT_DRAW = 0x2,
            CT_FUNC = 0x3,
            CT_SELECT = 0x4,
            CT_ILLUFACEEMOTION = 0x5,
            CT_FORCESELECTNEXTNPC = 0x6,
            CT_ILLUAVATAREMOTION = 0x7,
        };

    static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();

            Type enumType = typeof(CT);
            string enumFieldPrependedText = enumType.Name + "_";
            sb.AppendLine($"enum {enumType.Name} {{");

            foreach (var name in Enum.GetNames(enumType))
            {
                string normalizedName = name;
                if (normalizedName.StartsWith(enumFieldPrependedText))
                {
                    normalizedName = normalizedName.Substring(enumFieldPrependedText.Length);
                }

                if (char.IsDigit(normalizedName[0]))
                {
                    normalizedName = '_' + normalizedName;
                }

                sb.AppendLine($"\t{normalizedName} = {(int)Enum.Parse(enumType, name)},");
            }
            sb.AppendLine("}");

            Console.Write(sb.ToString());

            File.WriteAllText("enum.cs", sb.ToString());
        }
    }
}
