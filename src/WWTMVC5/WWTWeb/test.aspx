<%@ Page Language="C#" Async="true" ContentType="text/html" %>
<%@ Import Namespace="WWT.Providers" %>

<script runat="server">
	public void Page_Load(object sender, EventArgs e)
	{
		RequestProviderRunner.Run<testProvider>(this);
	}
</script>
