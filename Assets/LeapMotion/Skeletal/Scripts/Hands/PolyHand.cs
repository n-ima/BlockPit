/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary and  confidential.  Not for distribution.            *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement between *
* Leap Motion and you, your company or other organization.                     *
* Author: Matt Tytel
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

// A deforming very low poly count hand.
public class PolyHand : HandModel {

  void Start() {
  }

  public override void InitHand(Transform deviceTransform) {
    Hand leap_hand = GetLeapHand();
    Vector3 palm_normal = deviceTransform.TransformDirection(leap_hand.PalmNormal.ToUnity());
    Vector3 palm_direction = deviceTransform.TransformDirection(leap_hand.Direction.ToUnity());

    for (int f = 0; f < fingers.Length; ++f) {
      if (fingers[f] != null)
        fingers[f].InitFinger(deviceTransform, palm_normal, palm_direction);
    }
  }

  public override void UpdateHand(Transform deviceTransform) {
    InitHand(deviceTransform);
  }

}
