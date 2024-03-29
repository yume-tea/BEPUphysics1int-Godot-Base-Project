using System;
using BEPUphysics;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.CollisionTests.CollisionAlgorithms.GJK;
using BEPUphysics.Constraints;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysicsDemos
{
    /// <summary>
    /// This class contains a bunch of helper functions to set up the simulation for different goals.
    /// Try using some of the different profiles in the simulations to see the results.
    /// You can also play with the numbers in the profiles to see the results.
    /// 
    /// One configuration option that these functions do not take advantage of is changing the time step.
    /// By default, the Space.TimeStepSettings.TimeStepDuration is set to 1/60f.  This is pretty good for most simulations.
    /// However, sometimes, you may need to drop the rate down to 1/30f for performance reasons.  This harms the simulation quality quite a bit.
    /// On the other hand, the update rate can be increased to 1/120f or more, which vastly increases the simulation quality.
    /// 
    /// When using a non-60hz update rate it's a good idea to pass the elapsed gametime into the Space.Update method (you can find
    /// the demos's space update call in the Demo.cs Update function).  This will allow the engine to take as many timesteps are 
    /// necessary to keep up with passing time.  Just remember that if the simulation gets too hectic and the engine falls behind,
    /// performance will suffer a lot as it takes multiple expensive steps in a single frame trying to catch up.  In addition,
    /// since the number of time steps per frame isn't fixed when using internal time stepping, subtle unsmooth motion may creep in.
    /// This can be addressed by using the interpolation buffers.  Check out the Updating Asynchronously documentation for more information.
    /// [Asynchronous updating isn't needed to use internal time stepping, it's just a common use case.]
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Applies the default settings to the space.
        /// These values are what the engine starts with; they don't have to be applied unless you just want to get back to the defaults.
        /// This doesn't cover every single tunable field in the entire engine, just the main ones that this helper class is messing with.
        /// </summary>
        /// <param name="space">Space to configure.</param>
        public static void ApplyDefaultSettings(Space space)
        {
            MotionSettings.DefaultPositionUpdateMode = PositionUpdateMode.Discrete;
            SolverSettings.DefaultMinimumIterationCount = 1;
            space.Solver.IterationLimit = 10;
            GeneralConvexPairTester.UseSimplexCaching = false;
            MotionSettings.UseExtraExpansionForContinuousBoundingBoxes = false;

            //Set all the scaling settings back to their defaults.
            space.DeactivationManager.VelocityLowerLimit = (Fix64)0.26m;
            CollisionResponseSettings.MaximumPenetrationRecoverySpeed = 2;
            CollisionResponseSettings.BouncinessVelocityThreshold = 1;
            CollisionResponseSettings.StaticFrictionVelocityThreshold = (Fix64).2m;
            CollisionDetectionSettings.ContactInvalidationLength = (Fix64).1m;
            CollisionDetectionSettings.ContactMinimumSeparationDistance = (Fix64).03m;
            CollisionDetectionSettings.MaximumContactDistance = (Fix64).1m;
            CollisionDetectionSettings.DefaultMargin = (Fix64).04m;
            CollisionDetectionSettings.AllowedPenetration = (Fix64).01m;
            SolverSettings.DefaultMinimumImpulse = (Fix64)0.001m;

            //Adjust epsilons back to defaults.
            Toolbox.Epsilon = (Fix64)1e-7m;
            Toolbox.BigEpsilon = (Fix64)1e-5m;
            MPRToolbox.DepthRefinementEpsilon = (Fix64)1e-4m;
            MPRToolbox.RayCastSurfaceEpsilon = (Fix64)1e-9m;
            MPRToolbox.SurfaceEpsilon = (Fix64)1e-7m;
            PairSimplex.DistanceConvergenceEpsilon = (Fix64)1e-7m;
            PairSimplex.ProgressionEpsilon = (Fix64)1e-8m;

        }


        /// <summary>
        /// Applies slightly higher speed settings.
        /// The only change here is the default minimum iterations.
        /// In many simulations, having a minimum iteration count of 0 works just fine.
        /// It's a quick and still fairly robust way to get some extra performance.
        /// An example of where this might introduce some issues is sphere stacking.
        /// </summary>
        public static void ApplySemiSpeedySettings()
        {
            SolverSettings.DefaultMinimumIterationCount = 0;
        }

        /// <summary>
        /// Applies some low quality, high speed settings.
        /// The main benefit comes from the very low iteration cap.
        /// By enabling simplex caching, general convex collision detection
        /// gets a nice chunk faster, but some curved shapes lose collision detection robustness.
        /// </summary>
        /// <param name="space">Space to configure.</param>
        public static void ApplySuperSpeedySettings(Space space)
        {
            SolverSettings.DefaultMinimumIterationCount = 0;
            space.Solver.IterationLimit = 5;
            GeneralConvexPairTester.UseSimplexCaching = true;
        }

        /// <summary>
        /// Applies some higher quality settings.
        /// By using universal continuous collision detection, missed collisions
        /// will be much, much rarer.  This actually doesn't have a huge performance cost.
        /// The increased iterations put this as a midpoint between the normal and high stability settings.
        /// </summary>
        /// <param name="space">Space to configure.</param>
        public static void ApplyMediumHighStabilitySettings(Space space)
        {
            MotionSettings.DefaultPositionUpdateMode = PositionUpdateMode.Continuous;
            SolverSettings.DefaultMinimumIterationCount = 2;
            space.Solver.IterationLimit = 15;

        }

        /// <summary>
        /// Applies some high quality, low performance settings.
        /// By using universal continuous collision detection, missed collisions
        /// will be much, much rarer.  This actually doesn't have a huge performance cost.
        /// However, increasing the iteration limit and the minimum iterations to 5x the default
        /// will incur a pretty hefty overhead.
        /// On the upside, pretty much every simulation will be rock-solid.
        /// </summary>
        /// <param name="space">Space to configure.</param>
        public static void ApplyHighStabilitySettings(Space space)
        {
            MotionSettings.DefaultPositionUpdateMode = PositionUpdateMode.Continuous;
            MotionSettings.UseExtraExpansionForContinuousBoundingBoxes = true;
            SolverSettings.DefaultMinimumIterationCount = 5;
            space.Solver.IterationLimit = 50;

        }

        /// <summary>
        /// Scales the configuration settings for collision detection and response to handle
        /// a different scale interpretation.  For example, if you want to increase your gravity to -100 from -10 and consider a 5 unit wide box to be tiny,
        /// apply a scale of 10 to get the collision response and detection systems to match expectations.
        /// </summary>
        /// <param name="space">Space to configure.</param>
        /// <param name="scale">Scale to apply to relevant configuration settings.</param>
        public static void ApplyScale(Space space, Fix64 scale)
        {
            //Set all values to default values * scale.
            space.DeactivationManager.VelocityLowerLimit = (Fix64)0.26m * scale;
            CollisionResponseSettings.MaximumPenetrationRecoverySpeed = 2 * scale;
            CollisionResponseSettings.BouncinessVelocityThreshold = 1 * scale;
            CollisionResponseSettings.StaticFrictionVelocityThreshold = (Fix64).2m * scale;
            CollisionDetectionSettings.ContactInvalidationLength = (Fix64).1m * scale;
            CollisionDetectionSettings.ContactMinimumSeparationDistance = (Fix64).03m * scale;
            CollisionDetectionSettings.MaximumContactDistance = (Fix64).1m * scale;
            CollisionDetectionSettings.DefaultMargin = (Fix64).04m * scale;
            CollisionDetectionSettings.AllowedPenetration = (Fix64).01m * scale;

            //Adjust epsilons, too.
            Toolbox.Epsilon = (Fix64)1e-7m * scale;
            Toolbox.BigEpsilon = (Fix64)1e-5m * scale;
            MPRToolbox.DepthRefinementEpsilon = (Fix64)1e-4m * scale;
            MPRToolbox.RayCastSurfaceEpsilon = (Fix64)1e-9m * scale;
            MPRToolbox.SurfaceEpsilon = (Fix64)1e-7m * scale;
            PairSimplex.DistanceConvergenceEpsilon = (Fix64)1e-7m * scale;
            PairSimplex.ProgressionEpsilon = (Fix64)1e-8m * scale;

            //While not fully a size-related parameter, you may find that adjusting the SolverSettings.DefaultMinimumImpulse can help the simulation quality.
            //It is related to 'mass scale' instead of 'size scale.'
            //Heavy or effectively heavy objects will produce higher impulses and early out slower, taking more time than needed.
            //Light or effectively light objects will produce smaller impulses and early out faster, producing a lower quality result.
        }
    }
}
 