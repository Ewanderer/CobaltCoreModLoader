using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using CobaltCoreModding.Definitions.OverwriteItems;
using DemoMod.Actions;
using DemoMod.Cards;

namespace DemoMod
{
    public class ModManifest : IModManifest, ISpriteManifest, IDBManifest, IAnimationManifest, IDeckManifest, ICardManifest, ICardOverwriteManifest, ICharacterManifest, IGlossaryManifest
    {
        internal static int x = 0;
        private ExternalSprite? card_art_sprite;
        private ExternalAnimation? default_animation;
        private ExternalDeck? dracula_deck;
        private ExternalSprite? dracular_art;
        private ExternalSprite? dracular_border;
        private ExternalAnimation? mini_animation;
        private ExternalSprite? mini_dracula_sprite;
        private ExternalSprite? pinker_per_border_over_sprite;
        public IEnumerable<string> Dependencies => new string[0];
        public string Name => "EWanderer.DemoMod";

        public void BootMod(IModLoaderContact contact)
        {
            //Nothing to do here lol.
        }

        public void LoadManifest(IArtRegistry artRegistry)
        {
            {
                var sprite = new ExternalSprite("EWanderer.DemoMod.Patched_Cobalt_Core", new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\Sprites\\patched_cobalt_core.png"));
                artRegistry.RegisterArt(sprite, (int)Spr.cockpit_cobalt_core);
            }

            {
                pinker_per_border_over_sprite = new ExternalSprite("EWanderer.DemoMod.PinkerPeri.BorderOver", new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\Sprites\\border_over_pinker_peri.png"));
                if (!artRegistry.RegisterArt(pinker_per_border_over_sprite))
                    throw new Exception("Cannot register sprite.");
            }

            {
                card_art_sprite = new ExternalSprite("EWanderer.DemoMod.DemoCardArt", new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\Sprites\\Shield.png"));
                if (!artRegistry.RegisterArt(card_art_sprite))
                    throw new Exception("Cannot register sprite.");
                EWandererDemoCard.card_sprite = (Spr)(card_art_sprite.Id ?? throw new NullReferenceException());
            }
            {
                mini_dracula_sprite = new ExternalSprite("EWanderer.DemoMod.dracular.mini", new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\Sprites\\dracula_mini_0.png"));
                if (!artRegistry.RegisterArt(mini_dracula_sprite))
                    throw new Exception("Cannot register sprite.");
                EWandererDemoCard.card_sprite = (Spr)(mini_dracula_sprite.Id ?? throw new NullReferenceException());
            }
        }

        public void LoadManifest(IDbRegistry dbRegistry)
        {
        }

        public void LoadManifest(IAnimationRegistry registry)
        {
            default_animation = new ExternalAnimation("ewanderer.demomod.dracula.neutral", dracula_deck ?? throw new NullReferenceException(), "neutral", false, new ExternalSprite[] {
                ExternalSprite.GetRaw((int)Spr.characters_dracula_dracula_neutral_0),
                ExternalSprite.GetRaw((int)Spr.characters_dracula_dracula_neutral_1),
                ExternalSprite.GetRaw((int)Spr.characters_dracula_dracula_neutral_2),
                ExternalSprite.GetRaw((int)Spr.characters_dracula_dracula_neutral_3),
                ExternalSprite.GetRaw((int)Spr.characters_dracula_dracula_neutral_4),
            });

            registry.RegisterAnimation(default_animation);
            if (mini_dracula_sprite == null)
                throw new Exception();

            mini_animation = new ExternalAnimation("ewanderer.demomod.dracula.mini", dracula_deck, "mini", false, new ExternalSprite[] { mini_dracula_sprite });

            registry.RegisterAnimation(mini_animation);
        }

        public void LoadManifest(IDeckRegistry registry)
        {
            //make peri deck mod
            var art_default = ExternalSprite.GetRaw((int)Spr.cards_WaveBeam);
            var border = ExternalSprite.GetRaw((int)Spr.cardShared_border_ephemeral);

            var pinker_peri = new ExternalDeck("Ewanderer.DemoMod.PinkerPeri", System.Drawing.Color.Brown, System.Drawing.Color.Yellow, art_default, border, pinker_per_border_over_sprite);
            registry.RegisterDeck(pinker_peri, (int)Deck.peri);

            dracular_art = ExternalSprite.GetRaw((int)Spr.cards_colorless);
            dracular_border = ExternalSprite.GetRaw((int)Spr.cardShared_border_dracula);

            dracula_deck = new ExternalDeck("EWanderer.Demomod.DraculaDeck", System.Drawing.Color.Crimson, System.Drawing.Color.Purple, dracular_art ?? throw new NullReferenceException(), dracular_border ?? throw new NullReferenceException(), null);

            if (!registry.RegisterDeck(dracula_deck))
                return;
        }

        public void LoadManifest(ICardRegistry registry)
        {
            if (card_art_sprite == null)
                return;
            //make card meta data
            var card = new ExternalCard("Ewanderer.DemoMod.DemoCard", typeof(EWandererDemoCard), card_art_sprite, null);
            //add card name in english
            card.AddLocalisation("Schwarzmagier");
            //register card in the db extender.
            registry.RegisterCard(card);
        }

        public void LoadManifest(ICardOverwriteRegistry registry)
        {
            var new_meta = new CardMetaOverwrite("EWanderer.DemoMod.Meta")
            {
                Deck = ExternalDeck.GetRaw((int)Deck.dracula),
                DontLoc = false,
                DontOffer = false,
                ExtraGlossary = new string[] { "Help", "Why" },
                Rarity = (int)Rarity.rare,
                Unreleased = false,
                UpgradesTo = new int[] { (int)Upgrade.A, (int)Upgrade.B },
                WeirdCard = false
            };

            registry.RegisterCardMetaOverwrite(new_meta, typeof(CannonColorless).Name);

            var better_dodge = new PartialCardStatOverwrite("ewanderer.demomod.betterdodge", typeof(DodgeColorless)) { Cost = 0, Buoyant = true, Retain = true };

            registry.RegisterCardStatOverwrite(better_dodge);

            /*
            dbRegistry.RegisterCardMetaOverwrite(new_meta, typeof(CannonColorless).Name);
            var all_normal_cards = Assembly.GetAssembly(typeof(Card))?.GetTypes().Where(e => !e.IsAbstract && e.IsClass && e.IsSubclassOf(typeof(Card)));
            if (all_normal_cards != null)
            {
                foreach (var card_type in all_normal_cards)
                {
                    var zero_cost_overwrite = new PartialCardStatOverwrite("ewanderer.demomod.partialoverwrite." + card_type.Name, card_type);
                    zero_cost_overwrite.Cost = -1;
                    registry.RegisterCardStatOverwrite(zero_cost_overwrite);
                }
            }
            */
        }

        public void LoadManifest(ICharacterRegistry registry)
        {
            var dracular_spr = ExternalSprite.GetRaw((int)Spr.characters_dracula_dracula_neutral_0);

            var start_cards = new Type[] { typeof(DraculaCard), typeof(DraculaCard) };
            var playable_dracular_character = new ExternalCharacter("EWanderer.DemoMod.DracularChar", dracula_deck ?? throw new NullReferenceException(), dracular_spr, start_cards, new Type[0], default_animation ?? throw new NullReferenceException(), mini_animation ?? throw new NullReferenceException());
            playable_dracular_character.AddNameLocalisation("Count Dracula");
            playable_dracular_character.AddDescLocalisation("A vampire using blood magic to invoke the powers of the void.");
            registry.RegisterCharacter(playable_dracular_character);
        }

        public void LoadManifest(IGlossaryRegisty registry)
        {
            var icon = ExternalSprite.GetRaw((int)Spr.icons_ace);
            var glossary = new ExternalGlossary("Ewanderer.DemoMod.DemoCard.Glossary", "ewandererdemocard", false, ExternalGlossary.GlossayType.action, icon);
            glossary.AddLocalisation("en", "EWDemoaction", "Have all the cheesecake in the world!");
            registry.RegisterGlossary(glossary);
            EWandererDemoAction.glossary_item = glossary.Head;
        }
    }
}