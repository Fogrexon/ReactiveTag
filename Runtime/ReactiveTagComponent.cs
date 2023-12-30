using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReactiveTag
{
    /// <summary>
    /// ReactiveTagのコンポーネント本体
    /// </summary>
    public class ReactiveTagComponent: MonoBehaviour
    {
        /// <summary>
        /// タグのリスト管理
        /// </summary>
        private readonly List<Tag> _tags = new ();
        
        /// <summary>
        /// TagEffectのリスト
        /// </summary>
        private List<ITagEffect> _effects = new ();
        
        /// <summary>
        /// タグのリストを返す
        /// ただし中身の書き換えはメソッド経由したいので、ReadOnlyListで返す
        /// </summary>
        public IReadOnlyList<Tag> Tags => this._tags;
        
        private void EnableTag(Tag target)
        {
            if (target.Parent is not null)
            {
                EnableTag(target.Parent);
                var components = _effects.Where(effect => effect.TargetTag.Equals(target.Parent));
                foreach (var component in components)
                {
                    component.OnTagAttach();
                }
            }
        }
        
        private void DisableTag(Tag target)
        {
            if (target.Parent is not null)
            {
                var components = _effects.Where(effect => effect.TargetTag.Equals(target.Parent));
                foreach (var component in components)
                {
                    component.OnTagDetach();
                }
                DisableTag(target.Parent);
            }
        }
        
        /// <summary>
        /// そのタグを持っているか
        /// </summary>
        /// <param name="target">タグ</param>
        public bool Has(Tag target)
        {
            return _tags.Any(
                t => t.Equals(target)
                );
        }
        
        /// <summary>
        /// 子タグを持っているか
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool HasChildren(Tag target)
        {
            return _tags.Any(
                t => t.ChildrenOf(target)
            );
        }
        
        /// <summary>
        /// タグの追加
        /// </summary>
        /// <param name="target"></param>
        public void Add(Tag target)
        {
            EnableTag(target);
            _tags.Add(target);
        }
        
        /// <summary>
        /// タグの削除
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Remove(Tag target)
        {
            var flag = _tags.Remove(target);
            if (flag)
            {
                DisableTag(target);
            }

            return flag;
        }

        /// <summary>
        /// タグの全削除
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int RemoveAll(Tag target)
        {
            var removedCount = _tags.RemoveAll(t => t.Equals(target));
            for (var i=0;i<removedCount;i++)
            {
                DisableTag(target);
            }

            return removedCount;
        }
        
        /// <summary>
        /// 子タグの全削除
        /// </summary>
        /// <param name="target"></param>
        public void RemoveAllChildren(Tag target)
        {
            // 子タグを含めた削除
            var removingTags = _tags.Where(t => t.ChildrenOf(target));
            foreach (var removingTag in removingTags)
            {
                _tags.Remove(removingTag);
                DisableTag(target);
            }
        }

        public void Awake()
        {
            _effects = GetComponents<ITagEffect>().ToList();
        }
    }
}