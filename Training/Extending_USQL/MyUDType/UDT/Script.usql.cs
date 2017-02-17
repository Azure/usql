using Microsoft.Analytics.Interfaces;


namespace MyUDTExamples
{
    [SqlUserDefinedType(typeof(BitFormatter))]
    public struct Bits
    {
        System.Collections.BitArray bitarray;

        public Bits(string s)
        {
            this.bitarray = new System.Collections.BitArray(s.Length);
            for (int i = 0; i<s.Length; i++)
            {
                char c = s[s.Length-i-1];
                this.bitarray[i] = (c == '1' ? true : false);
            }
        }

        public int ToInteger()
        {
            int value = 0;
            for (int i = 0; i < this.bitarray.Length; i++)
            {
                if (bitarray[i])
                {
                    value += (int)System.Math.Pow(2, i);
                }
            }
            return value;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder(this.bitarray.Length);
            for (int i = 0; i < this.bitarray.Length; i++)
            {
                sb.Append(this.bitarray[i] ? "1" : "0");
            }
            return sb.ToString();
        }
    }

    public class BitFormatter : Microsoft.Analytics.Interfaces.IFormatter<Bits>
    {
        public BitFormatter()
        {
        }

        public void Serialize(
                        Bits instance,
                        IColumnWriter writer,
                        ISerializationContext context)
        {
            using (var w = new System.IO.StreamWriter(writer.BaseStream))
            {
                var bitstring = instance.ToString();
                w.Write(bitstring);
                w.Flush();
            }
        }

        public Bits Deserialize(
                         IColumnReader reader,
                         ISerializationContext context)
        {
            using (var w = new System.IO.StreamReader(reader.BaseStream))
            {
                string bitstring = w.ReadToEnd();
                var bits = new Bits(bitstring);
                return bits;
            }
        }
    }

}
