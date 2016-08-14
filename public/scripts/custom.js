var username;

$(document).ready(function () {
	$("#submit").click(function () {
		$.post("/new", "steam=" + $("#steam").val() + "&mobile=" + $("#mobile").val(), function () {
			$("#created").fadeIn();
		});
	});
});
