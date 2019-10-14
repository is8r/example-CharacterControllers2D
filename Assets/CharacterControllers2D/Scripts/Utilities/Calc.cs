using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterControllers2D.Utilities
{
    public static class Calc
    {
        //_vectorから_direction方向のVector値を取り出す
        public static Vector2 ExtractDotVector(Vector2 _vector, Vector2 _direction)
        {
            if (System.Math.Abs(_direction.sqrMagnitude - 1f) > 0.001f)
            {
                _direction.Normalize();
            }

            float _amount = Vector2.Dot(_vector, _direction);

            return _direction * _amount;
        }

        //「_speed」および「_deltaTime」を使用して、「_ currentValue」から「_targetValue」に向かって浮動小数点値を増分
        public static float IncrementValueTowardTarget(float _currentValue, float _speed, float _deltaTime, float _targetValue)
        {
            if (System.Math.Abs(_currentValue - _targetValue) < 0.001f)
            {
                return _currentValue;
            }

            float _sign = Mathf.Sign(_targetValue - _currentValue);
            float _remainingDistance = Mathf.Abs(_targetValue - _currentValue);

            if (Mathf.Abs(_speed * Time.deltaTime * _sign) > _remainingDistance)
            {
                return _targetValue;
            }
            else
            {
                return _currentValue + _speed * Time.deltaTime * _sign;
            }
        }

        //「_speed」と「_deltaTime」を使用して、ベクトルの長さを「_targetLength」に向かって増分
        public static Vector2 IncrementVectorLengthTowardTargetLength(Vector2 _currentVector, float _speed, float _deltaTime, float _targetLength)
        {
            float _currentLength = _currentVector.magnitude;
            Vector2 _normalizedVector = _currentVector / _currentLength;

            if (System.Math.Abs(_currentLength - _targetLength) < 0.001f)
            {
                return _currentVector;
            }

            float _newLength = IncrementValueTowardTarget(_currentLength, _speed, _deltaTime, _targetLength);

            return _normalizedVector * _newLength;
        }

        //'_direction'と同じ方向を指しているベクトルからすべてのパーツを削除
        public static Vector2 RemoveDotVector(Vector2 _vector, Vector2 _direction)
        {
            if (System.Math.Abs(_direction.sqrMagnitude - 1) > 0.001f)
            {
                _direction.Normalize();
            }

            float _amount = Vector2.Dot(_vector, _direction);

            _vector -= _direction * _amount;

            return _vector;
        }

        ////'_vector_1'と '_vector_2'の間の符号付き角度（-180〜+180の範囲）を計算
        //public static float GetAngle(Vector3 _vector1, Vector3 _vector2, Vector3 _planeNormal)
        //{
        //    float _angle = Vector3.Angle(_vector1, _vector2);
        //    float _sign = Mathf.Sign(Vector3.Dot(_planeNormal, Vector3.Cross(_vector1, _vector2)));
        //    float _signedAngle = _angle * _sign;
        //    return _signedAngle;
        //}
    }
}
