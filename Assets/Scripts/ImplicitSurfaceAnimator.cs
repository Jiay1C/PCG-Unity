using UnityEngine;

public class ImplicitSurfaceAnimator : MonoBehaviour
{
    public RuntimeAnimatorController baseController;
    public AnimationClip animationClip;

    private Animator _animator;
    private RuntimeAnimatorController _controller;

    public ImplicitSurfaceAnimator Attach(GameObject target)
    {
        ImplicitSurfaceAnimator targetAnimator = target.GetComponent<ImplicitSurfaceAnimator>();
        if (targetAnimator == null)
        {
            targetAnimator = target.AddComponent<ImplicitSurfaceAnimator>();
        }

        targetAnimator.baseController = baseController;
        targetAnimator.animationClip = animationClip;

        return targetAnimator;
    }

    public void Play()
    {
        if (animationClip == null)
        {
            Debug.LogWarning("AnimationClip is not assigned.");
            return;
        }

        if (_animator == null)
        {
            _animator = gameObject.GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
            }
        }

        if (_controller == null)
        {
            if (baseController == null)
            {
                Debug.LogError("Base controller is not assigned.");
            }
            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = baseController;
            overrideController["DefaultAnimation"] = animationClip;
            _controller = overrideController;
        }

        _animator.runtimeAnimatorController = _controller;

        Debug.Log($"{gameObject.name} Start Playing Animation: {animationClip.name}");
    }

    public void Stop()
    {
        if (_animator != null)
        {
            _animator.runtimeAnimatorController = null;
        }
    }
}
