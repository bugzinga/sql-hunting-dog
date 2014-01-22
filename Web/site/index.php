<!DOCTYPE html>
<html>
	<head>
<?php include "header.php" ?>
	</head>
	<body>
		<center>
			<table>
				<tr>
					<td style="padding-right: 7px">
						<a href="/">
							<img src="img/logo.jpg" width="60" height="60" />
						</a>
					</td>
					<td>
						<a href="/">
							<h1>SQL HUNTING DOG</h1>
						</a>
						<center class="menu">
							<span class="active">What</span>
							<a href="why" class="inactive">Why</a>
							<a href="how" class="inactive">How</a>
							<!--a href="../who" class="inactive">Who</a-->
							<a href="http://blog.sql-hunting-dog.com" class="inactive">Blog</a>
						</center>
					</td>
					<td style="padding-left: 11px">
						<a href="https://bitbucket.org/bugzinga/sql-hunting-dog/downloads/HuntingDog-2.3.4.msi">
							<img src="img/download.png" width="50" height="50" /><br/>
						</a>
					</td>
				</tr>
<?php include "donate.php" ?>
<?php include "code.php" ?>
			</table>
			<div id="content">
				<h2>Quick Search Tool (AddIn) for Microsoft SQL Management Studio 2008/2012</h2>
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
			<div class="signature">2013 (c) <a href="mailto:sqlhuntingdog@gmail.com">SQL Hunting Dog</a></div>
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
