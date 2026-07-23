using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinHerlper : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    

    public RuntimeAnimatorController GetSkin()
    {
        return _animator.runtimeAnimatorController;
    }
}
