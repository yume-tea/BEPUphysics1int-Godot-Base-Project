﻿using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Materials;
using BEPUutilities;

namespace BEPUphysicsDemos.Demos
{
    /// <summary>
    /// Boxes slide and spheres bounce.
    /// </summary>
    public class CoefficientsDemo : StandardDemo
    {
        /// <summary>
        /// Constructs a new demo.
        /// </summary>
        /// <param name="game">Game owning this demo.</param>
        public CoefficientsDemo(DemosGame game)
            : base(game)
        {
            //The material blender defines how the friction and bounciness values are computed between objects.
            //It defaults to multiplicative, but for the purposes of this demo, we'll switch it to use the smaller friction and the larger bounciness.
            MaterialManager.MaterialBlender = delegate(Material a, Material b, out InteractionProperties properties)
                {
                    properties.KineticFriction = MathHelper.Min(a.KineticFriction, b.KineticFriction);
                    properties.StaticFriction = MathHelper.Min(a.StaticFriction, b.StaticFriction);
                    properties.Bounciness = MathHelper.Max(a.Bounciness, b.Bounciness);
                };
            //Special blenders can be defined between specific Material instances by adding entries to the MaterialManager.MaterialInteractions dictionary.

            //Create the ground
            Entity toAdd = new Box(new Vector3(0, -.5m, 0), 20, 1, 20);
            Space.Add(toAdd);
            //Bouncy balls 
            toAdd = new Sphere(new Vector3(-8, 10, 0), 1, 1);
            toAdd.Material = new Material(.8m, .8m, .95m);
            Space.Add(toAdd);
            toAdd = new Sphere(new Vector3(-5, 10, 0), 1, 1);
            toAdd.Material = new Material(.8m, .8m, .5m);
            Space.Add(toAdd);
            toAdd = new Sphere(new Vector3(-2, 10, 0), 1, 1);
            toAdd.Material = new Material(.8m, .8m, .25m);
            Space.Add(toAdd);
            //Slide-y boxes
            toAdd = new Box(new Vector3(9, 1, 9), 1, 1, 1, 1);
            toAdd.Material = new Material(0, 0, 0);
            toAdd.LinearVelocity = new Vector3(-5, 0, 0);
            Space.Add(toAdd);
            toAdd = new Box(new Vector3(9, 1, 5), 1, 1, 1, 1);
            toAdd.Material = new Material(.1m, .1m, 0);
            toAdd.LinearVelocity = new Vector3(-5, 0, 0);
            Space.Add(toAdd);
            toAdd = new Box(new Vector3(9, 1, -5), 1, 1, 1, 1);
            toAdd.Material = new Material(.2m, .2m, 0);
            toAdd.LinearVelocity = new Vector3(-5, 0, 0);
            Space.Add(toAdd);
            toAdd = new Box(new Vector3(9, 1, -9), 1, 1, 1, 1);
            toAdd.Material = new Material(.5m, .5m, 0);
            toAdd.LinearVelocity = new Vector3(-5, 0, 0);
            Space.Add(toAdd);
            game.Camera.Position = new Vector3(0, 2, 30);

        }

        /// <summary>
        /// Gets the name of the simulation.
        /// </summary>
        public override string Name
        {
            get { return "Bounciness and Friction"; }
        }

        public override void CleanUp()
        {
            MaterialManager.MaterialBlender = MaterialManager.DefaultMaterialBlender;
            base.CleanUp();
        }
    }
}