﻿
namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dragonfly.NetModels;
using Umbraco.Cms.Core.Models;

public enum MessageType
{
	Info,
	Warning,
	Error
}

public interface IPageData
{
	StatusMessage? Status { get; set; }

	string SpecialMessage { get; set; }

	MessageType SpecialMessageType { get; set; }

	string ViewName { get; set; }

}

public class PageData<T> : IPageData
	  where T : class
{
	public T? DataModel { get; set; }

	public PageData(T? Data)
	{
		DataModel = Data;
		Status = new StatusMessage(false);
		SpecialMessage = "";
		SpecialMessageType = MessageType.Info;
		ViewName = "";
	}


	#region Implementation of IPageData

	public StatusMessage Status { get; set; }

	public string SpecialMessage { get; set; }

	public MessageType SpecialMessageType { get; set; }
	public string ViewName { get; set; }

	#endregion
}
