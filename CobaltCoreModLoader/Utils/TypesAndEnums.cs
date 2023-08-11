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

        public static ExternalSprite GetOriginalSprite(int sprVal)
        {
            //check if sprval is valid
            if (Enum.IsDefined(TypesAndEnums.SprType, sprVal))
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
