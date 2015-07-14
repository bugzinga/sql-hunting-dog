<!DOCTYPE html>
<html>
<?php include "../header.php" ?>


<body>
		<center>
			<table>
	<?php
		include "../menu.php";
		include "../donate.php";
	?>
			</table>
				<div id="content" class="article">
					<h2>
						<?php
							   $visitor_email = $_POST["email"];  

							   if( filter_var($visitor_email, FILTER_VALIDATE_EMAIL) )
							   {
						    	   $email_from = "sqlhuntingdog@gmail.com"; 
								   $email_subject = "New subscription "; 
								   $email_body = "New person subscribed $visitor_email.\n".

								   $to = "sqlhuntingdog@gmail.com";
								 
								   $headers = "From: $email_from \r\n";
								  
								   mail($to,$email_subject,$email_body,$headers);

								   echo "Thank you for subscribing";
							   }
							   else
							   {
							   		 echo "Email address is invalid";

							   }
							
						     	
						?>
					</h2>
				<p></p>
				<p></p>
				<p></p>

				
				</div>
			<div class="signature">2015 (c) <a href="mailto:sqlhuntingdog@gmail.com">SQL Hunting Dog</a></div>
		</center>
	</body>
</html>
