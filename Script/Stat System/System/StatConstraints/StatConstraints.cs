namespace GeneralGameDevKit.StatSystem
{
    /// <summary>
    /// Class that defines stat value constraints
    /// </summary>
    public abstract class StatConstraints
    {
        public string targetStatID;
        public bool isBaseStatConstraintsActivated;
        public bool isApplyStatConstraintsActivated;
        
        public abstract float ProcessBaseStat(StatSystemCore targetSystem, float value);
        public abstract float ProcessApplyStat(StatSystemCore targetSystem, float value);
    }
}