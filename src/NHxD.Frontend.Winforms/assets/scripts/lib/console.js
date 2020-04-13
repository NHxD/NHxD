{
	var console = {}

	console.element = null

	console.exists = function()
	{
		return this.element
	}

	console.log = function()
	{
		if (!this.exists())
		{
			return
		}

		for (var i = 0, len = arguments.length; i < len; ++i)
		{
			var arg = arguments[i]
			var p = document.createElement("P")
			var txt = document.createTextNode(arg)
			var span = document.createElement("SPAN")

			span.appendChild(txt)
			p.appendChild(span)

			this.element.appendChild(p)
		}
	}

	console.logHtml = function(htmlContent)
	{
		if (!this.exists())
		{
			return
		}

		var p = document.createElement("P")
		var span = document.createElement("SPAN")

		span.innerHTML = htmlContent
		p.appendChild(span)

		this.element.appendChild(p)
	}

	console.clear = function()
	{
		if (!this.exists())
		{
			return
		}

		this.element.innerHTML = ""
	}

	console.create = function(appendOrPrepend)
	{
		appendOrPrepend = appendOrPrepend || true

		if (this.exists())
		{
			return
		}

		var div = document.createElement("DIV")

		if (appendOrPrepend)
		{
			document.body.appendChild(div)
		}
		else
		{
			document.body.insertBefore(div, document.body.firstChild)
		}

		div.id = "console"
		div.oncontextmenu = function(event) { console.clear() }

		this.element = div
	}

	console.init = function()
	{
		this.element = document.getElementById("console")
	}

	console.remove = function()
	{
		if (!this.exists())
		{
			return
		}

		document.body.removeChild(this.element)
	}
}
