using CobaltCoreModding.Components.Services;

namespace CobaltCoreModding.Components.Utils
{
    /// <summary>
    /// Just have all the relevant types from cobalt core types and enum conveter setup here.
    /// </summary>
    internal class TypesAndEnums
    {
        private static Type? __artifact_meta_type = null;
        private static Type? __artifact_type = null;
        private static Type? __card_meta_type = null;

        private static Type? __card_type = null;

        private static Type? __cobalt_color_type = null;

        private static Type? __db_type = null;

        private static Type? __deck_def_type = null;

        private static Type? __deck_type = null;

        private static Type? __enum_extensions_type = null;

        private static Type? __mutil_type = null;
        private static Type? __new_run_options_type = null;

        private static Type? __part_type = null;
        private static Type? __rarity_type = null;

        private static Type? __ship_type = null;
        private static Type? __spr_type = null;

        private static Type? __sprite_path_type = null;
        private static Type? __starter_Deck_type = null;

        private static Type? __startership_type = null;
        private static Type? __state_type = null;
        private static Type? __status_def_type = null;
        private static Type? __status_type = null;
        private static Type? __story_vars_type = null;

        private static Type? __ttglossary_type = null;
        private static Type? __upgrade_type = null;
        private static Type? __ptype_type = null;

        private static Type? __node_type_type = null;
        private static Type? __story_node_type = null;
        private static Type? __story_type = null;
        private static Type? __say_type = null;
        private static Type? __say_switch_type = null;
        private static Type? __instruction_type = null;

        public static Type PTypeType
        {
            get
            {
                if (__ptype_type != null)
                    return __ptype_type;
                return __ptype_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("PType") ?? throw new Exception("PType type not found");
            }
        }


        public static Type ArtifactMetaType
        {
            get
            {
                if (__artifact_meta_type != null)
                    return __artifact_meta_type;
                return __artifact_meta_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("ArtifactMeta") ?? throw new Exception("ArtifactMeta type not found");
            }
        }

        public static Type ArtifactType
        {
            get
            {
                if (__artifact_type != null)
                    return __artifact_type;
                return __artifact_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Artifact") ?? throw new Exception("Artifact type not found");
            }
        }

        public static Type CardMetaType
        {
            get
            {
                if (__card_meta_type != null)
                    return __card_meta_type;
                return __card_meta_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("CardMeta") ?? throw new Exception("card meta type not found");
            }
        }

        public static Type CardType
        {
            get
            {
                if (__card_type != null)
                    return __card_type;
                return __card_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Card") ?? throw new Exception("card type not found");
            }
        }

        public static Type CobaltColorType
        {
            get
            {
                if (__cobalt_color_type != null)
                    return __cobalt_color_type;
                return __cobalt_color_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Color") ?? throw new Exception("Color type not found");
            }
        }

        public static Type DbType
        {
            get
            {
                if (__db_type != null) return __db_type;

                return __db_type = (CobaltCoreHandler.CobaltCoreAssembly?.GetType("DB") ?? throw new Exception("DB not found."));
            }
        }

        public static Type DeckDefType
        {
            get
            {
                if (__deck_def_type != null)
                    return __deck_def_type;
                return __deck_def_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("DeckDef") ?? throw new Exception("DeckDef type not found");
            }
        }

        public static Type DeckType
        {
            get
            {
                if (__deck_type != null)
                    return __deck_type;
                return __deck_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Deck") ?? throw new Exception("Deck type not found");
            }
        }

        public static Type EnumExtensionsType
        {
            get
            {
                if (__enum_extensions_type != null)
                    return __enum_extensions_type;
                return __enum_extensions_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("EnumExtensions") ?? throw new Exception("EnumExtensions type not found");
            }
        }

        public static Type MutilType
        {
            get
            {
                if (__mutil_type != null)
                    return __mutil_type;
                return __mutil_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Mutil") ?? throw new Exception("Mutil type not found");
            }
        }

        public static Type NewRunOptionsType
        {
            get
            {
                if (__new_run_options_type != null)
                    return __new_run_options_type;
                return __new_run_options_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("NewRunOptions") ?? throw new Exception("NewRunOptions type not found");
            }
        }

        public static Type PartType
        {
            get
            {
                if (__part_type != null)
                    return __part_type;
                return __part_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Part") ?? throw new Exception("Part type not found");
            }
        }

        public static Type RarityType
        {
            get
            {
                if (__rarity_type != null)
                    return __rarity_type;
                return __rarity_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Rarity") ?? throw new Exception("Rarity type not found");
            }
        }

        public static Type ShipType
        {
            get
            {
                if (__ship_type != null)
                    return __ship_type;
                return __ship_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Ship") ?? throw new Exception("Ship type not found");
            }
        }

        public static Type SpritePathType
        {
            get
            {
                if (__sprite_path_type != null)
                    return __sprite_path_type;
                return __sprite_path_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("SpritePath") ?? throw new Exception("SpritePath type not found");
            }
        }

        public static Type SprType
        {
            get
            {
                if (__spr_type != null)
                    return __spr_type;
                return __spr_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Spr") ?? throw new Exception("spr type not found");
            }
        }

        public static Type StarterDeckType
        {
            get
            {
                if (__starter_Deck_type != null)
                    return __starter_Deck_type;
                return __starter_Deck_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("StarterDeck") ?? throw new Exception("StarterDeck type not found");
            }
        }

        public static Type StarterShipType
        {
            get
            {
                if (__startership_type != null)
                    return __startership_type;
                return __startership_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("StarterShip") ?? throw new Exception("StarterShip type not found");
            }
        }

        public static Type StateType
        {
            get
            {
                if (__state_type != null)
                    return __state_type;
                return __state_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("State") ?? throw new Exception("State type not found");
            }
        }

        public static Type StatusDefType
        {
            get
            {
                if (__status_def_type != null)
                    return __status_def_type;
                return __status_def_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("StatusDef") ?? throw new Exception("StatusDef type not found.");
            }
        }

        public static Type StatusType
        {
            get
            {
                if (__status_type != null)
                    return __status_type;
                return __status_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Status") ?? throw new Exception("Status type not found.");
            }
        }

        public static Type StoryVarsType
        {
            get
            {
                if (__story_vars_type != null)
                    return __story_vars_type;
                return __story_vars_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("StoryVars") ?? throw new Exception("StoryVars type not found");
            }
        }

        public static Type TTGlossaryType
        {
            get
            {
                if (__ttglossary_type != null)
                    return __ttglossary_type;
                return __ttglossary_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("TTGlossary") ?? throw new Exception("TTGlossary type not found.");
            }
        }

        public static Type UpgradeType
        {
            get
            {
                if (__upgrade_type != null)
                    return __upgrade_type;
                return __upgrade_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Upgrade") ?? throw new Exception("Upgrade type not found");
            }
        }

        public static Type NodeTypeType
        {
            get
            {
                if (__node_type_type != null)
                    return __node_type_type;
                return __node_type_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("NodeType") ?? throw new Exception("NodeType type not found");
            }
        }

        public static Type StoryNodeType
        {
            get 
            {
                if (__story_node_type != null)
                    return __story_node_type;
                return __story_node_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("StoryNode") ?? throw new Exception("StoryNode type not found");
            }
        }

        public static Type StoryType
        {
            get
            {
                if (__story_type != null)
                    return __story_type;
                return __story_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Story") ?? throw new Exception("Story type not found");
            }
        }

        public static Type SayType
        {
            get
            {
                if (__say_type != null)
                    return __say_type;
                return __say_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Say") ?? throw new Exception("Say type not found");
            }
        }

        public static Type SaySwitchType
        {
            get
            {
                if (__say_switch_type != null)
                    return __say_switch_type;
                return __say_switch_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("SaySwitch") ?? throw new Exception("SaySwitch type not found");
            }
        }

        public static Type InstructionType
        {
            get
            {
                if (__instruction_type != null)
                    return __instruction_type;
                return __instruction_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Instruction") ?? throw new Exception("Instruction type not found");
            }
        }

        public static object? IntToDeck(int? deck_id)
        {
            if (deck_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(DeckType, deck_id), DeckType);
        }

        public static object? IntToRarity(int? rarity_id)
        {
            if (rarity_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(RarityType, rarity_id), RarityType);
        }

        public static object? IntToSpr(int? spr_id)
        {
            if (spr_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(SprType, spr_id), SprType);
        }

        public static object? IntToStatus(int? status_id)
        {
            if (status_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(StatusType, status_id), StatusType);
        }

        public static object? IntToPType(int? ptype_id)
        {
            if (ptype_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(PTypeType, ptype_id), PTypeType);
        }

        public static object? IntToUpgrade(int? upgrade_id)
        {
            if (upgrade_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(UpgradeType, upgrade_id), UpgradeType);
        }

        public static object? IntToNodeType(int? nodetype_id)
        {
            if(nodetype_id == null)
                return null;
            return Convert.ChangeType(Enum.ToObject(NodeTypeType, nodetype_id), NodeTypeType);
        }
    }
}