﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        private readonly ShakeContainer shakeContainer;

        // Must be set to update IsHovered as it's used in relax mdo to detect osu hit objects.
        public override bool HandlePositionalInput => true;

        protected override float SamplePlaybackPosition => HitObject.X / OsuPlayfield.BASE_SIZE.X;

        /// <summary>
        /// Whether this <see cref="DrawableOsuHitObject"/> can be hit.
        /// If non-null, judgements will be ignored (resulting in a shake) whilst the function returns false.
        /// </summary>
        public Func<DrawableHitObject, double, bool> CheckHittable;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
            base.AddInternal(shakeContainer = new ShakeContainer
            {
                ShakeDuration = 30,
                RelativeSizeAxes = Axes.Both
            });

            Alpha = 0;
        }

        // Forward all internal management to shakeContainer.
        // This is a bit ugly but we don't have the concept of InternalContent so it'll have to do for now. (https://github.com/ppy/osu-framework/issues/1690)
        protected override void AddInternal(Drawable drawable) => shakeContainer.Add(drawable);
        protected override void ClearInternal(bool disposeChildren = true) => shakeContainer.Clear(disposeChildren);
        protected override bool RemoveInternal(Drawable drawable) => shakeContainer.Remove(drawable);

        protected sealed override double InitialLifetimeOffset => HitObject.TimePreempt;

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ??= GetContainingInputManager() as OsuInputManager;

        protected virtual void Shake(double maximumLength) => shakeContainer.Shake(maximumLength);

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Idle:
                    // Manually set to reduce the number of future alive objects to a bare minimum.
                    LifetimeStart = HitObject.StartTime - HitObject.TimePreempt;
                    break;
            }
        }

        /// <summary>
        /// Causes this <see cref="DrawableOsuHitObject"/> to get missed, disregarding all conditions in implementations of <see cref="DrawableHitObject.CheckForResult"/>.
        /// </summary>
        public void MissForcefully() => ApplyResult(r => r.Type = HitResult.Miss);

        protected override JudgementResult CreateResult(Judgement judgement) => new OsuJudgementResult(HitObject, judgement);
    }
}
