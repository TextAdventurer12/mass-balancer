using System;
using System.Collections.Generic;
using System.Reflection;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty.Skills;

namespace MassBalancer
{
    public class Constant
    {
        private readonly FieldInfo field;
        public double Value
        {
            get { return (field.GetValue(null) as double?) ?? 0; }
            set { field.SetValue(null, value); }
        }
        public Constant(Type type, string name)
        {
            field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
        public override string ToString()
            => Value.ToString();
        public static Constant GetFromProperty(PropertyInfo info, Constants owner)
            => info.GetValue(owner) as Constant;
    }
    public class Constants
    {
        public static double performanceMultiplier = 1;
        public Constant aimMultiplier { get; set; }
        public Constant speedMultiplier { get; set; }
        public Constant speedStrainDecayBase { get; set; }
        public Constant aimStrainDecayBase { get; set; }

        public Constants()
        {
            aimMultiplier = new Constant(typeof(Aim), "skillMultiplier");
            speedMultiplier = new Constant(typeof(Speed), "skillMultiplier");
            speedStrainDecayBase = new Constant(typeof(Speed), "strainDecayBase");
            aimStrainDecayBase = new Constant(typeof(Aim), "strainDecayBase");
        }
        public override string ToString()
        {
            string output = "";
            PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (PropertyInfo property in props)
                output += $"{property.Name}: {property.GetValue(this)}\n";
            return output;
        }
    }
}