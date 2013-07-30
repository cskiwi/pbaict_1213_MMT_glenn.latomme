using System.Diagnostics;
using Engine._2D;
using Soccer.Messaging;

namespace Soccer.Game {
    public abstract class BaseGameEntity {
        public enum DefaultEntityTypeEnum {
            DefaultEntityType = -1
        }

        private static int _mINextValidId;
        protected Vector2D VPosition = new Vector2D();
        protected Vector2D VScale = new Vector2D();
        protected double dBoundingRadius;
        private bool _mBTag;
        private int _mIType;
        private int _mId;

        protected BaseGameEntity(int id) {
            dBoundingRadius = 0.0;
            VScale = new Vector2D(1.0, 1.0);
            _mIType = (int) DefaultEntityTypeEnum.DefaultEntityType;
            _mBTag = false;
            SetId(id);
        }

        private void SetId(int val) {
            Debug.Assert((val >= _mINextValidId), "<BaseGameEntity::SetID>: invalid ID");

            _mId = val;

            _mINextValidId = _mId + 1;
        }


        public virtual void Dispose() {}

        public virtual void Update() {}

        public abstract void Render();

        public virtual bool HandleMessage(Telegram msg) {
            return false;
        }

        //use this to grab the next valid ID
        public static int GetNextValidId() {
            return _mINextValidId;
        }

        //this can be used to reset the next ID
        public static void ResetNextValidId() {
            _mINextValidId = 0;
        }

        public Vector2D Pos() {
            return VPosition;
        }

        public void SetPos(Vector2D newPos) {
            VPosition = newPos;
        }

        public double BRadius() {
            return dBoundingRadius;
        }

        public void SetBRadius(double r) {
            dBoundingRadius = r;
        }

        public int Id() {
            return _mId;
        }

        public bool IsTagged() {
            return _mBTag;
        }

        public void Tag() {
            _mBTag = true;
        }

        public void UnTag() {
            _mBTag = false;
        }

        public Vector2D Scale() {
            return VScale;
        }

        public void SetScale(Vector2D val) {
            dBoundingRadius *= Vector2D.MaxOf(val)/Vector2D.MaxOf(VScale);
            VScale = val;
        }

        public void SetScale(double val) {
            dBoundingRadius *= (val/Vector2D.MaxOf(VScale));
            VScale = new Vector2D(val, val);
        }

        public int EntityType() {
            return _mIType;
        }

        public void SetEntityType(int newType) {
            _mIType = newType;
        }
    }
}