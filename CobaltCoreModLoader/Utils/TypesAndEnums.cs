using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModLoader.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModLoader.Utils
{
    /// <summary>
    /// Just have all the relevant types from cobalt core types and enum conveter setup here.
    /// </summary>
    internal class TypesAndEnums
    {

        private static Type? __card_meta_type = null;

        public static Type CardMetaType
        {
            get
            {
                if (__card_meta_type != null)
                    return __card_meta_type;
                return __card_meta_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("CardMeta") ?? throw new Exception("card meta type not found");
            }
        }

        private static Type? __card_type = null;

        public static Type CardType
        {
            get
            {
                if (__card_type != null)
                    return __card_type;
                return __card_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Card") ?? throw new Exception("card type not found");
            }
        }

        public static ExternalSprite GetOriginalSprite(int sprVal)
        {
            //check if sprval is valid
            if (!Enum.IsDefined(TypesAndEnums.SprType, sprVal))
            {
                throw new Exception("Unkown spr from cobalt core game requested.");
            }
            return ExternalSprite.GetRaw(sprVal);
        }

        private static Type? __spr_type = null;

        public static Type SprType
        {
            get
            {
                if (__spr_type != null)
                    return __spr_type;
                return __spr_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Spr") ?? throw new Exception("spr type not found");
            }
        }

        private static Type? __deck_type = null;

        public static Type DeckType
        {
            get
            {
                if (__deck_type != null)
                    return __deck_type;
                return __deck_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Deck") ?? throw new Exception("Deck type not found");
            }
        }

        private static Type? __deck_def_type = null;

        public static Type DeckDefType
        {
            get
            {
                if (__deck_def_type != null)
                    return __deck_def_type;
                return __deck_def_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("DeckDef") ?? throw new Exception("DeckDef type not found");
            }
        }

        private static Type? __cobalt_color_type = null;


        public static Type CobaltColorType
        {
            get
            {
                if (__cobalt_color_type != null)
                    return __cobalt_color_type;
                return __cobalt_color_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Color") ?? throw new Exception("Color type not found");
            }
        }

        private static Type? __upgrade_type = null;


        public static Type UpgradeType
        {
            get
            {
                if (__upgrade_type != null)
                    return __upgrade_type;
                return __upgrade_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Upgrade") ?? throw new Exception("Upgrade type not found");
            }
        }

        private static Type? __rarity_type = null;


        public static Type RarityType
        {
            get
            {
                if (__rarity_type != null)
                    return __rarity_type;
                return __rarity_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Rarity") ?? throw new Exception("Rarity type not found");
            }
        }

        public static object? IntToRarity(int? rarity_id)
        {
            if (rarity_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(RarityType, rarity_id), RarityType);
        }

        public static object? IntToUpgrade(int? upgrade_id)
        {
            if (upgrade_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(UpgradeType, upgrade_id), UpgradeType);
        }

        public static object? IntToSpr(int? spr_id)
        {
            if (spr_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(SprType, spr_id), SprType);
        }

        public static object? IntToDeck(int? deck_id)
        {
            if (deck_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(DeckType, deck_id), DeckType);
        }

    }
}
