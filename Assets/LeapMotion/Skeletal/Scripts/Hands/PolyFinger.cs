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
using System;
using Leap;

public class PolyFinger : FingerModel {

  const int MAX_SIDES = 12;
  const int TRIANGLE_INDICES_PER_QUAD = 6;
  const int VERTICES_PER_QUAD = 4;

  public Material material;
  public int sides = 4;
  public bool smoothNormals = false;
  public float startingAngle = 0.0f;
  
  private Mesh mesh_;

  void Update() {
    if (mesh_ == null)
      mesh_ = new Mesh();

    mesh_.RecalculateBounds();
    mesh_.RecalculateNormals();
    Graphics.DrawMesh(mesh_, Matrix4x4.identity, material, 0);
  }

  protected Vector3 GetJointNormal(int joint) {
    if (joint == 0)
      return Vector3.zero;
    Vector3 prev = -GetBoneDirection(joint - 1);
    Vector3 next = GetBoneDirection(joint);
    Vector3 normal = (prev + next) / 2.0f;
    normal.Normalize();
    return normal;
  }

  protected void UpdateMeshSmooth(Transform deviceTransform,
                                  Vector3 palm_normal, Vector3 palm_direction) {
    Vector3[] vertices = mesh_.vertices;
    Array.Resize(ref vertices, sides * NUM_JOINTS);

    int triangle_index = 0;
    int[] triangles = mesh_.triangles;
    Array.Resize(ref triangles, TRIANGLE_INDICES_PER_QUAD * sides * NUM_JOINTS);

    for (int i = 0; i < NUM_BONES; ++i) {
      Vector3 joint_position = deviceTransform.TransformPoint(GetJointPosition(i));
      Vector3 joint_direction = 0.5f * (deviceTransform.TransformDirection(GetBoneDirection(i - 1)) +
                                        deviceTransform.TransformDirection(GetBoneDirection(i)));
      Vector3 joint_normal = deviceTransform.TransformDirection(GetJointNormal(i));

      for (int s = 0; s < sides; ++s) {
        float angle = startingAngle + s * 360.0f / sides;
        Vector3 offset = Quaternion.AngleAxis(angle, joint_direction) * joint_normal;
        vertices[s + NUM_JOINTS * i] = joint_position + 0.2f * offset;

        if (i != NUM_BONES - 1) {
          triangles[triangle_index++] = s + NUM_JOINTS * i;
          triangles[triangle_index++] = ((s + 1) % sides) + NUM_JOINTS * i;
          triangles[triangle_index++] = s + NUM_JOINTS * (i + 1);

          triangles[triangle_index++] = ((s + 1) % sides) + NUM_JOINTS * (i + 1);
          triangles[triangle_index++] = s + NUM_JOINTS * (i + 1);
          triangles[triangle_index++] = ((s + 1) % sides) + NUM_JOINTS * i;
        }
      }
    }

    mesh_.vertices = vertices;
    mesh_.triangles = triangles;
  }

  protected void UpdateMesh(Transform deviceTransform,
                            Vector3 palm_normal, Vector3 palm_direction) {
    int vertex_index = 0;
    Vector3[] vertices = mesh_.vertices;
    Array.Resize(ref vertices, VERTICES_PER_QUAD * sides * NUM_JOINTS);

    int triangle_index = 0;
    int[] triangles = mesh_.triangles;
    Array.Resize(ref triangles, TRIANGLE_INDICES_PER_QUAD * sides * NUM_JOINTS);

    for (int i = 0; i < NUM_JOINTS; ++i) {
      Vector3 joint_position = deviceTransform.TransformPoint(GetJointPosition(i));
      Vector3 next_joint_position = deviceTransform.TransformPoint(GetJointPosition(i + 1));
      Vector3 joint_direction = 0.5f * (deviceTransform.TransformDirection(GetBoneDirection(i - 1)) +
                                        deviceTransform.TransformDirection(GetBoneDirection(i)));

      Vector3 bone_normal = deviceTransform.TransformDirection(GetJointNormal(i));
      Vector3 next_bone_normal = deviceTransform.TransformDirection(GetJointNormal(i + 1));

      for (int s = 0; s < sides; ++s) {
        Vector3 next_joint_direction = 0.5f * (deviceTransform.TransformDirection(GetBoneDirection(i)) +
                                               deviceTransform.TransformDirection(GetBoneDirection(i + 1)));
        float from_angle = startingAngle + s * 360.0f / sides;
        float to_angle = startingAngle + (s + 1) * 360.0f / sides;
        Vector3 from_offset1 = Quaternion.AngleAxis(from_angle, joint_direction) * bone_normal;
        Vector3 to_offset1 = Quaternion.AngleAxis(to_angle, joint_direction) * bone_normal;
        Vector3 from_offset2 = Quaternion.AngleAxis(from_angle, next_joint_direction) * next_bone_normal;
        Vector3 to_offset2 = Quaternion.AngleAxis(to_angle, next_joint_direction) * next_bone_normal;

        if (i != NUM_JOINTS - 1) {
          triangles[triangle_index++] = vertex_index;
          triangles[triangle_index++] = vertex_index + 1;
          triangles[triangle_index++] = vertex_index + 2;

          triangles[triangle_index++] = vertex_index + 3;
          triangles[triangle_index++] = vertex_index + 2;
          triangles[triangle_index++] = vertex_index + 1;
        }

        vertices[vertex_index++] = joint_position + 0.2f * from_offset1;
        vertices[vertex_index++] = joint_position + 0.2f * to_offset1;
        vertices[vertex_index++] = next_joint_position + 0.2f * from_offset2;
        vertices[vertex_index++] = next_joint_position + 0.2f * to_offset2;
      }
    }

    mesh_.vertices = vertices;
    mesh_.triangles = triangles;
  }

  public override void InitFinger(Transform deviceTransform,
                                  Vector3 palm_normal, Vector3 palm_direction) {
    if (mesh_ == null)
      mesh_ = new Mesh();

    if (smoothNormals)
      UpdateMeshSmooth(deviceTransform, palm_normal, palm_direction);
    else
      UpdateMesh(deviceTransform, palm_normal, palm_direction);
  }

  public override void UpdateFinger(Transform deviceTransform,
                                    Vector3 palm_normal, Vector3 palm_direction) {
    InitFinger(deviceTransform, palm_normal, palm_direction);
  }
}
