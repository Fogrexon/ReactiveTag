using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace ReactiveTag
{
    /// <summary>
    /// タグのベースとなるクラス
    /// これを継承してタグを作るが、それはyamlによる自動生成を想定している
    /// </summary>
    public abstract class Tag
    {
        private Guid _id;
        private string _name;
        [CanBeNull] private Tag _parent;
        private Tag[] _children;
        
        /// <summary>
        /// タグのID
        /// 比較はこのIDで行う
        /// </summary>
        public Guid Id => this._id;
        
        /// <summary>
        /// タグの名前
        /// </summary>
        public string Name => this._name;
        
        /// <summary>
        /// 親タグ
        /// </summary>
        public Tag Parent => this._parent;
        
        /// <summary>
        /// 子タグのリスト
        /// </summary>
        /// <param name="id"></param>
        public ReadOnlyCollection<Tag> Children => Array.AsReadOnly(_children);
        
        public Tag(Guid id, string name)
        {
            this._id = id;
            this._name = name;
        }

        /// <summary>
        /// タグが有効化された際に呼ばれる
        /// </summary>
        /// <param name="component">タグのつけ外しを行うコンポーネント</param>
        public void OnEnable(ReactiveTagComponent component)
        {
            
        }

        /// <summary>
        /// タグが無効化された際に呼ばれる
        /// </summary>
        /// <param name="component">タグのつけ外しを行うコンポーネント</param>
        public void OnDisable(ReactiveTagComponent component)
        {
            
        }
        
        /// <summary>
        /// 厳密に同じタグかどうか
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Equals(Tag tag)
        {
            return this._id.Equals(tag.Id);
        }
        
        /// <summary>
        /// tagの子供かどうかを再帰的に調べる
        /// </summary>
        /// <param name="tag">親タグ</param>
        /// <returns></returns>
        public bool ChildrenOf(Tag tag)
        {
            if (this._parent == null)
            {
                return false;
            }
            
            if (this._parent.Equals(tag))
            {
                return true;
            }
            
            return this._parent.ChildrenOf(tag);
        }

        public override string ToString()
        {
            if (this._parent == null)
            {
                return this._name;
            }
            return $"{this._parent.ToString()}/{this._name}";
        }
    }
}