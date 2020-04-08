namespace NeatRider.Model
{
    public static class Config
    {
        public static int MAX_GENES = 100000;
        public static double SURVIVORS = .5;
        public static int NUM_CLIENTS = 1000;
        public static int ITERATIONS = 1000;

        public static double C1 = 1;
        public static double C2 = 1;
        public static double C3 = .4;

        public static double RANDOM_WEIGHT_STRENGTH = 1d;
        public static double SHIFT_WEIGHT_STRENGTH = .1;

        public static double MUTATE_NODE_PROB = .0025;
        public static double MUTATE_LINK_PROB = .005;
        public static double TOGGLE_PROB = .01;
        public static double RANDOM_WEIGHT_PROB = .025;
        public static double SHIFT_WEIGHT_PROB = .8;

        public static double IN_SPECIES_THRESHOLD = 3d;
    }
}