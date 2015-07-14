<!DOCTYPE html>
<html>
<?php include "../header.php" ?>
	<body>
		<center>
			<table>
<?php
	$active = "why";
	include "../menu.php";
	include "../donate.php";
?>
			</table>
			<div id="content" class="article">
				<h2>Why default SSMS Object Browser kills time of people working with SQL Server</h2>
				<p>
					This tool was created for developers and database administrators who work
					with Microsoft SQL Server Management Studio 2008 (or 2012, or 2014) every day.
					Unfortunately, the default Object Explorer is hard to use, especially when you
					need to quickly find a desired table or procedure.
					SSMS forces the user to perform 7-10 different actions to navigate to the table and
					then another 3-5 actions just to query that table to inspect the table schema.
					All those unnecessary activities consume a huge amount of time of the SQL Server developer or administrator.
				</p>
				<p>
					Let us have a look how many actions we have to perform to open a stored procedure
					for investigation assuming that SQL Server Database contains more than 100
					stored procedures (this is quite typical for the real production systems).
				</p>
				<p style="text-align: center">
					<img class="screenshot" src="../img/why/image001.png" style="width: 1187px; position: relative; left: -343px" />
				</p>
				<ol>
					<li>
						The user has to collapse the Object Explorer tree and expand &quot;Stored
						Procedures&quot; folder. This usually takes 3-5 mouse clicks:<br />
						<p style="text-align: center">
							<img class="screenshot" src="../img/why/image003.png" />
						</p>
					</li>
					<li>
						Then the user has to set the filtering criteria, because it is very hard to select
						a stored procedure from the Object Explorer plain list, epecially if the list
						contains more than 100 records (2 clicks):<br />
						<p style="text-align: center">
							<img class="screenshot" src="../img/why/image005.png" style="width: 550px; position: relative; left: -25px" />
						</p>
					</li>
					<li>
						Then the user can define the filter criteria and click &quot;OK&quot; (2 clicks):<br />
						<p style="text-align: center">
							<img class="screenshot" src="../img/why/image007.png" style="width: 543px; position: relative; left: -21px" />
						</p>
					</li>
					<li>
						Then SSMS shows only matching stored procedures, and now the user
						can select from a smaller set.
					</li>
					<li>
						Now the user has to tell SSMS that he wants to open the stored procedure for
						modification. It costs him 2 clicks (right-click on the Stored Procedure name,
						then navigation to &quot;MODIFY&quot; sub menu, and left-click).
					</li>
					<li>
						Hooray! Now SQL Server Management Studio shows the stored procedure body to the
						user. It costed the user about 10 clicks and about 10 seconds of his time.
						What a waste...
					</li>
				</ol>
				<p>
					How can you do it quicker? Imagine that you could locate and open a stored
					procedure in SSMS with the only click? SQL Hunting Dog was developed to help
					developers and database administrators work with SQL Server Management Studio
					more efficiently. It gives "google-like" search inside SSMS:
				</p>
				<ol>
					<li>
						You type the stored procedure name (or part of the name)<br />
						<p style="text-align: center">
							<img class="screenshot" src="../img/why/image009.png" style="width: 477px" />
						</p>
					</li>
					<li>
						You select it from the matching list<br />
						<p style="text-align: center">
							<img class="screenshot" src="../img/why/image011.png" style="width: 459px" />
						</p>
					</li>
					<li>
						You open the stored procedure with a single keystroke<br />
						<p style="text-align: center">
							<img class="screenshot" src="../img/why/image013.png" style="width: 459px" />
						</p>
					</li>
				</ol>
				<p>
					Amazingly simple! And it integrates inside existing instance of SQL Server
					Management Studio. Hunting Dog is compatible with the following SSMS versions:
				</p>
				<p>
					<ul>
						<li>SQL Server Management Studio 2008 (32 bit and 64 bit)</li>
						<li>SQL Server Management Studio 2008R2 (32 bit and 64 bit)</li>
						<li>SQL Server Management Studio 2012 (32 bit and 64 bit)</li>
						<li>SQL Server Management Studio 2014 (32 bit and 64 bit)</li>
					</ul>
				</p>
				<p>
					From my personal experience and from experience of other senior
					developers this plug-in is a must have if you a professional SQL Server developer
					and you are using SSMS in you everyday work.
				</p>
			</div>
			<div class="signature">2015 (c) <a href="mailto:sqlhuntingdog@gmail.com">SQL Hunting Dog</a></div>
		</center>
	</body>
</html>
