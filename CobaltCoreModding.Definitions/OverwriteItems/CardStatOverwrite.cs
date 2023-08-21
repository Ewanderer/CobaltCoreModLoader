using System.Reflection;

namespace CobaltCoreModding.Definitions.OverwriteItems
{
    public class ActiveCardStatOverwrite : CardStatOverwrite
    {
        public ActiveCardStatOverwrite(string globalName, Type cardType, Func<object, object, object> get_stat_function) : base(globalName, cardType)
        {
            GetStatFunction = get_stat_function;
        }

        /// <summary>
        /// A function taking cobaltcore.State and cobaltcore.carddata and returns CobaltCore.CardData.
        /// </summary>
        public Func<object, object, object> GetStatFunction { get; init; }
    }

    public abstract class CardStatOverwrite
    {
        public CardStatOverwrite(string globalName, Type cardType)
        {
            if (string.IsNullOrEmpty(globalName)) throw new ArgumentNullException(nameof(globalName));

            GlobalName = globalName;
            CardType = cardType;
        }

        public Type CardType { get; init; }
        public string GlobalName { get; init; }
    }

    public class PartialCardStatOverwrite : CardStatOverwrite
    {
        public static Type? __sprType;

        private static FieldInfo? __art_field;

        private static FieldInfo? __artTint_field;

        private static FieldInfo? __buoyant_field;

        private static FieldInfo? __cost_field;

        private static FieldInfo? __description_field;

        private static FieldInfo? __exhaust_field;

        private static FieldInfo? __flippable_field;

        private static FieldInfo? __floppable_field;

        private static FieldInfo? __infinite_field;

        private static FieldInfo? __recycle_field;

        private static FieldInfo? __retain_field;

        private static FieldInfo? __singleUse_field;

        private static FieldInfo? __temporary_field;

        private static FieldInfo? __unplayable_field;

        private static FieldInfo? __unremovableAtShops_field;

        public PartialCardStatOverwrite(string globalName, Type cardType) : base(globalName, cardType)
        {
        }

        /// <summary>
        /// will be set by mod loader for ApplyOverwrite use.
        /// </summary>
        public static Type SprType
        {
            get => __sprType ?? throw new NullReferenceException("SprType not set in cardStatOverwrite"); set
            {
                if (__sprType == null) __sprType = value;
            }
        }

        public int? Art { get; set; }
        public string? ArtTint { get; set; }
        public bool? Buoyant { get; set; }
        public int? Cost { get; set; }
        public string? Description { get; set; }
        public bool? Exhaust { get; set; }
        public bool? Flippable { get; set; }
        public bool? Floppable { get; set; }
        public bool? Infinite { get; set; }
        public bool? Recycle { get; set; }
        public bool? Retain { get; set; }
        public bool? SingleUse { get; set; }
        public bool? Temporary { get; set; }
        public bool? Unplayable { get; set; }
        public bool? UnremovableAtShops { get; set; }
        private static Type? __card_stat_type { get; set; }

        private static FieldInfo? art_field
        {
            get
            {
                if (__art_field != null)
                {
                    return __art_field;
                }

                return __art_field = card_stat_type.GetField("art") ?? throw new Exception("CardStat art field not found");
            }
        }

        private static FieldInfo? artTint_field
        {
            get
            {
                if (__artTint_field != null)
                {
                    return __artTint_field;
                }

                return __artTint_field = card_stat_type.GetField("artTint") ?? throw new Exception("CardStat artTint field not found");
            }
        }

        private static FieldInfo? buoyant_field
        {
            get
            {
                if (__buoyant_field != null)
                {
                    return __buoyant_field;
                }

                return __buoyant_field = card_stat_type.GetField("buoyant") ?? throw new Exception("CardStat buoyant field not found");
            }
        }

        private static Type card_stat_type { get => __card_stat_type ?? throw new NullReferenceException("no card stat type registered"); set => __card_stat_type = value; }

        private static FieldInfo? cost_field
        {
            get
            {
                if (__cost_field != null)
                {
                    return __cost_field;
                }

                return __cost_field = card_stat_type.GetField("cost") ?? throw new Exception("CardStat cost field not found");
            }
        }

        private static FieldInfo? description_field
        {
            get
            {
                if (__description_field != null)
                {
                    return __description_field;
                }

                return __description_field = card_stat_type.GetField("description") ?? throw new Exception("CardStat description field not found");
            }
        }

        private static FieldInfo? exhaust_field
        {
            get
            {
                if (__exhaust_field != null)
                {
                    return __exhaust_field;
                }

                return __exhaust_field = card_stat_type.GetField("exhaust") ?? throw new Exception("CardStat exhaust field not found");
            }
        }

        private static FieldInfo? flippable_field
        {
            get
            {
                if (__flippable_field != null)
                {
                    return __flippable_field;
                }

                return __flippable_field = card_stat_type.GetField("flippable") ?? throw new Exception("CardStat flippable field not found");
            }
        }

        private static FieldInfo? floppable_field
        {
            get
            {
                if (__floppable_field != null)
                {
                    return __floppable_field;
                }

                return __floppable_field = card_stat_type.GetField("floppable") ?? throw new Exception("CardStat floppable field not found");
            }
        }

        private static FieldInfo? infinite_field
        {
            get
            {
                if (__infinite_field != null)
                {
                    return __infinite_field;
                }

                return __infinite_field = card_stat_type.GetField("infinite") ?? throw new Exception("CardStat infinite field not found");
            }
        }

        private static FieldInfo? recycle_field
        {
            get
            {
                if (__recycle_field != null)
                {
                    return __recycle_field;
                }

                return __recycle_field = card_stat_type.GetField("recycle") ?? throw new Exception("CardStat recycle field not found");
            }
        }

        private static FieldInfo? retain_field
        {
            get
            {
                if (__retain_field != null)
                {
                    return __retain_field;
                }

                return __retain_field = card_stat_type.GetField("retain") ?? throw new Exception("CardStat retain field not found");
            }
        }

        private static FieldInfo? singleUse_field
        {
            get
            {
                if (__singleUse_field != null)
                {
                    return __singleUse_field;
                }

                return __singleUse_field = card_stat_type.GetField("singleUse") ?? throw new Exception("CardStat singleUse field not found");
            }
        }

        private static FieldInfo? temporary_field
        {
            get
            {
                if (__temporary_field != null)
                {
                    return __temporary_field;
                }

                return __temporary_field = card_stat_type.GetField("temporary") ?? throw new Exception("CardStat temporary field not found");
            }
        }

        private static FieldInfo? unplayable_field
        {
            get
            {
                if (__unplayable_field != null)
                {
                    return __unplayable_field;
                }

                return __unplayable_field = card_stat_type.GetField("unplayable") ?? throw new Exception("CardStat unplayable field not found");
            }
        }

        private static FieldInfo? unremovableAtShops_field
        {
            get
            {
                if (__unremovableAtShops_field != null)
                {
                    return __unremovableAtShops_field;
                }

                return __unremovableAtShops_field = card_stat_type.GetField("unremovableAtShops") ?? throw new Exception("CardStat unremovableAtShops field not found");
            }
        }

        public void ApplyOverwrite(ref object card_data)
        {
            if (__card_stat_type == null)
                __card_stat_type = card_data.GetType();
            if (Cost != null)
                cost_field?.SetValue(card_data, Cost.Value);
            if (Exhaust != null)
                exhaust_field?.SetValue(card_data, Exhaust.Value);
            if (Retain != null)
                retain_field?.SetValue(card_data, Retain.Value);
            if (Recycle != null)
                recycle_field?.SetValue(card_data, Recycle.Value);
            if (Infinite != null)
                infinite_field?.SetValue(card_data, Infinite.Value);
            if (Unplayable != null)
                unplayable_field?.SetValue(card_data, Unplayable.Value);
            if (Temporary != null)
                temporary_field?.SetValue(card_data, Temporary.Value);
            if (Flippable != null)
                flippable_field?.SetValue(card_data, Flippable.Value);
            if (Floppable != null)
                floppable_field?.SetValue(card_data, Floppable.Value);
            if (Buoyant != null)
                buoyant_field?.SetValue(card_data, Buoyant.Value);
            if (SingleUse != null)
                singleUse_field?.SetValue(card_data, SingleUse.Value);
            if (UnremovableAtShops != null)
                unremovableAtShops_field?.SetValue(card_data, UnremovableAtShops.Value);
            if (Description != null)
                description_field?.SetValue(card_data, Description);
            if (ArtTint != null)
                artTint_field?.SetValue(card_data, ArtTint);
            //convert to Spr
            if (Art != null)
            {
                var art_spr_val = Convert.ChangeType(Enum.ToObject(SprType, Art.Value), SprType);
                if (art_spr_val != null)
                    art_field?.SetValue(card_data, art_spr_val);
            }
        }
    }
}