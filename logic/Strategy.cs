using PlayerDotNet.models;

namespace PlayerDotNet.logic
{
    public abstract class Strategy
    {
        
        private static GameState gameStateGlob;

        private static int Distance(Position p1, Position p2)
        {
            return (int)Math.Sqrt( Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2) );
        }
        private static int UnitsAtTarget(Base src, Base target, uint units)
        {
            var dist = Distance(src.Position, target.Position);
            var grace = dist - gameStateGlob.Config.Paths.GracePeriod;

            return (int)(units - grace * gameStateGlob.Config.Paths.DeathRate);
        }
        
        
        public static PlayerAction Decide(GameState? gameState)
        {
            PlayerAction action = new PlayerAction();
            if (gameState == null) return action;
            
            gameStateGlob = gameState;
            
            List<Base> Enemy  = new List<Base>();
            List<Base> Player = new List<Base>();
            foreach (var gameBase in gameState.Bases)
            {
                if (gameBase.Player == gameState.Game.Player)
                {
                    Player.Add(gameBase);
                }
                else
                {
                    Enemy.Add(gameBase);
                }
            }
            
            // Consolidate
            // Upgrade
            const int UpgradeTill = 5;
            // Expand
            const int MinUnitsAtBase = 100;
            
            // Get Homebase if Needed
            // Select new home on Death
            if (Cache.Home != null && Cache.Home.Population == 0 ) Cache.Home = null;
            
            Cache.Home ??= Player.OrderBy(b => b.Population).First();

            if (Cache.Home.Level < UpgradeTill)
            {
                //Consolidate & Upgrade
                if (Cache.Home.UnitsUntilUpgrade <= Cache.Home.Population)
                {
                    // Upgrade O Clock
                    action.Dest = Cache.Home.Uid;
                    action.Src = Cache.Home.Uid;
                    action.Amount = Cache.Home.UnitsUntilUpgrade;
                    return action;
                }
                // Consolidate Things Home if able
                var tmp = Consolidate(Player);
                if (tmp != null) return tmp;
            }
            
            
            // EXPAND
            uint AvailbaleUnits = Cache.Home.Population - MinUnitsAtBase;
            if (AvailbaleUnits > 0)
            {
                // There is a Chance
                // But is there Really??
                var target = Enemy.Where(b => UnitsAtTarget(Cache.Home, b, AvailbaleUnits) > 0)
                    .OrderBy(b => UnitsAtTarget(Cache.Home, b, AvailbaleUnits)).First();
                
                var DamagePercent = UnitsAtTarget(Cache.Home, target, AvailbaleUnits) / target.Population * 100;
                if (DamagePercent > Random.Shared.Next(0,100))
                {
                    action.Dest = target.Uid;
                    action.Src = Cache.Home.Uid;
                    action.Amount = AvailbaleUnits;
                    return action; 
                }
                
               
            }
            
            
            // Can We Upgrade? DO IT!!!!
            if (Cache.Home.UnitsUntilUpgrade <= Cache.Home.Population)
            {
                // Upgrade O Clock
                action.Dest = Cache.Home.Uid;
                action.Src = Cache.Home.Uid;
                action.Amount = Cache.Home.UnitsUntilUpgrade;
                return action;
            }
            
            // Hmm what to do maybe chance
            var maybeConsol = Consolidate(Player);
            if (maybeConsol != null && Random.Shared.Next(100) > 50)
            {
                return maybeConsol;
            }
            
            
            
            return action;
        }

        private static PlayerAction? Consolidate(List<Base> Player)
        {
            PlayerAction action  = new PlayerAction();
            var otherOwnBases = Player.Where(b => b.Uid != Cache.Home.Uid && UnitsAtTarget(b, Cache.Home, b.Population) > 0)
                .OrderBy(b => UnitsAtTarget(b, Cache.Home, b.Population));
            if ( !otherOwnBases.Any()) return null;
            
            var consolidateSrc = otherOwnBases.First();
            
            // Consolidate
            action.Dest = Cache.Home.Uid;
            action.Src = consolidateSrc.Uid;
            action.Amount = consolidateSrc.Population;
            return action;
        }
    }
}
