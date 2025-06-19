﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedFox.Graphics3D.Skeletal
{
    /// <summary>
    /// A class to handle sampling an <see cref="SkeletonAnimation"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public class SkeletonAnimationSampler : AnimationSampler
    {
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Gets or Sets the targets.
        /// </summary>
        public List<SkeletonAnimationTargetSampler> TargetSamplers { get; set; } = [];

        public SkeletonAnimationSampler(string name, SkeletonAnimation animation, Skeleton skeleton) : base(name, animation)
        {
            Skeleton = skeleton;
            skeleton.InitializeAnimationTransforms();

            var stack = new Stack<SkeletonAnimationTargetSampler>();

            // First pull roots
            foreach (var bone in skeleton.EnumerateRoots())
            {
                stack.Push(CreateTargetSampler(bone, animation, null));
            }

            // Now traverse
            while (stack.Count > 0)
            {
                var popper = stack.Pop();

                // Add children for this
                foreach (var child in popper.Bone.Children)
                {
                    stack.Push(CreateTargetSampler(child, animation, popper));
                }

                TargetSamplers.Add(popper);
            }
        }

        public SkeletonAnimationSampler(string name, SkeletonAnimation animation, Skeleton skeleton, AnimationPlayer player) : this(name, animation, skeleton)
        {
            player.AddLayer(this);
        }


        public void SetTransformType(TransformType type)
        {
            foreach (var targetSampler in TargetSamplers)
            {
                targetSampler.TransformType = type;
            }
        }

        public override void UpdateObjects()
        {
            foreach (var sampler in TargetSamplers)
            {
                sampler.Update();
            }
        }

        private SkeletonAnimationTargetSampler CreateTargetSampler(SkeletonBone bone, SkeletonAnimation animation, SkeletonAnimationTargetSampler? parent)
        {
            // Attempt to find a target
            var targetIndex = animation.Targets.FindIndex(x =>
            {
                return x.BoneName.Equals(
                    bone.Name,
                    StringComparison.CurrentCultureIgnoreCase);
            });

            var transformSpace = parent == null ? animation.TransformSpace : parent.TransformSpace;
            var transformType = parent == null ? animation.TransformType : parent.TransformType;

            if (parent is not null && parent.Target is not null)
            {
                if (parent.Target.ChildTransformType != TransformType.Unknown)
                    transformType = parent.Target.ChildTransformType;
            }

            SkeletonAnimationTarget? target = null;

            if (targetIndex != -1)
            {
                target = animation.Targets[targetIndex];
            }

            return new(
                this,
                bone,
                target,
                transformType,
                transformSpace);
        }
    }
}
