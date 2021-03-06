﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110227
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100227
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.Domain.Caching;

namespace Rafy.WPF.Behaviors
{
    /// <summary>
    /// 列表在切换时，保存实体的行为
    /// </summary>
    public class SaveAsChangedBehavior : ViewBehavior
    {
        private object _lastWarnedObjId = null;

        public new ListLogicalView View
        {
            get
            {
                var listView = base.View as ListLogicalView;
                if (listView == null) throw new InvalidOperationException();

                return listView;
            }
        }

        protected override void OnAttach()
        {
            this.View.SelectedItemChanged += View_SelectedItemChanged;
        }

        internal bool SuppressSaveAction { get; set; }

        /// <summary>
        /// 切换左边视图
        /// 
        /// 保存选择的旧实体，并异步获取新的实体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_SelectedItemChanged(object sender, SelectedEntityChangedEventArgs e)
        {
            if (this.SuppressSaveAction) return;

            //保存选择的旧实体
            var oldEntity = e.OldItem;
            if (oldEntity != null)
            {
                // oldEntity.ApplyEdit();  //编辑时切换到列表时对象处于编辑状态
                //如果不是新增后立即删除的可更改对象则自动保存
                if (oldEntity.IsDirty)
                {
                    var broken = oldEntity.Validate();
                    if (broken.Count == 0)
                    {
                        //oldEntity.MergeOldObject((oldEntity as ISavable).Save() as BusinessBase);
                        RF.Save(oldEntity, EntitySaveType.DiffSave);
                    }
                    else
                    {
                        bool warn = true;
                        if (e.NewItem != null)
                        {
                            var id = e.NewItem.Id;
                            warn = !object.Equals(id, this._lastWarnedObjId);
                        }
                        if (warn && broken.Count > 0)
                        {
                            this._lastWarnedObjId = oldEntity.Id;

                            string description = string.Format(@"
旧对象没有填写完整，系统没有保存！
{0}

是否切换到该对象，继续编辑？".Translate(), broken.ToString());
                            var result = App.MessageBox.Show(description, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (result == MessageBoxResult.Yes)
                            {
                                this.View.Current = oldEntity as Entity;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}