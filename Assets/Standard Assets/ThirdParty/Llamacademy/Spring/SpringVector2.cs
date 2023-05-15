using UnityEngine;

namespace DeepDreams.ThirdPartyAssets.Llamacademy.Spring
{
    public class SpringVector2 : BaseSpring<Vector2>
    {
        private readonly FloatSpring XSpring = new FloatSpring();
        private readonly FloatSpring YSpring = new FloatSpring();

        public override float Damping
        {
            get => base.Damping;
            set
            {
                XSpring.Damping = value;
                YSpring.Damping = value;
                base.Damping = value;
            }
        }

        public override float Stiffness
        {
            get => base.Stiffness;
            set
            {
                XSpring.Stiffness = value;
                YSpring.Stiffness = value;
                base.Stiffness = value;
            }
        }

        public override Vector2 InitialVelocity
        {
            get => new Vector2(XSpring.InitialVelocity, YSpring.InitialVelocity);
            set
            {
                XSpring.InitialVelocity = value.x;
                YSpring.InitialVelocity = value.y;
            }
        }

        public override Vector2 StartValue
        {
            get => new Vector2(XSpring.StartValue, YSpring.StartValue);
            set
            {
                XSpring.StartValue = value.x;
                YSpring.StartValue = value.y;
            }
        }
        public override Vector2 EndValue
        {
            get => new Vector2(XSpring.EndValue, YSpring.EndValue);
            set
            {
                XSpring.EndValue = value.x;
                YSpring.EndValue = value.y;
            }
        }

        public override Vector2 CurrentVelocity
        {
            get => new Vector2(XSpring.CurrentVelocity, YSpring.CurrentVelocity);
            set
            {
                XSpring.CurrentVelocity = value.x;
                YSpring.CurrentVelocity = value.y;
            }
        }

        public override Vector2 CurrentValue
        {
            get => new Vector2(XSpring.CurrentValue, YSpring.CurrentValue);
            set
            {
                XSpring.CurrentValue = value.x;
                YSpring.CurrentValue = value.y;
            }
        }

        public override Vector2 Evaluate(float DeltaTime)
        {
            CurrentValue = new Vector2(XSpring.Evaluate(DeltaTime), YSpring.Evaluate(DeltaTime));
            CurrentVelocity = new Vector2(XSpring.CurrentVelocity, YSpring.CurrentVelocity);
            return CurrentValue;
        }

        public override void Reset()
        {
            XSpring.Reset();
            YSpring.Reset();
        }

        public override void UpdateEndValue(Vector2 Value, Vector2 Velocity)
        {
            XSpring.UpdateEndValue(Value.x, Velocity.x);
            YSpring.UpdateEndValue(Value.y, Velocity.y);
        }
    }
}