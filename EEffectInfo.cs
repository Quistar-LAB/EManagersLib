namespace EManagersLib {
    public static class EEffectInfo {
        public static uint GetBuilding(InstanceID id) {
            uint building = id.Building;
            if (building == 0) {
                id.GetBuildingProp32(out building, out int num);
            }
            return building;
        }
    }
}
