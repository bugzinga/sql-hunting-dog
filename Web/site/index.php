<!DOCTYPE html>
<html>
<?php include "header.php" ?>
	<body>
		<center>
			<table>
<?php
	$active = "main";
	include "menu.php";
	include "donate.php";
	include "code.php";
?>
			</table>
			<div id="content">
				<h2>Quick Search Tool (AddIn) for Microsoft SQL Management Studio 2008/2012/2014</h2>
				<div>
					<div class="theme-default slider-wrapper">
						<div id="slider" class="nivoSlider">
							<img src="img/screenshot-1.jpg" />
							<img src="img/screenshot-2.jpg" />
							<img src="img/screenshot-3.jpg" />
						</div>
					</div>
				</div>
				<div style="width: 500px; text-align: left">
					<ul>
						<li>Quickly find tables, stored procedure, functions and views</li>
						<li>Completely removes the pain of clunky Object Explorer</li>
						<li>Switch between different servers and databases</li>
						<li>Perform common operation (select data, modify table, design table, etc.) with ease</li>
					</ul>
				</div>
			</div>
			
			<div style="width: 500px; text-align: left">
				<p>
				SQL Hunting Dog is a free SQL Server tool. It works as an Addin and gives you quick search and smooth navigation. Do not use it very often , otherwise you will become addictive.
				</p>
			</div>
			<div class="signature">2015 (c) <a href="mailto:sqlhuntingdog@gmail.com">SQL Hunting Dog</a></div>
		</center>
		<script type="text/javascript">
			$(window).load(function() {
				    $('#slider').nivoSlider({
					    	controlNav: true,
					    	pauseTime: 5500
				  	  });
				});
		</script>
	</body>
</html>
