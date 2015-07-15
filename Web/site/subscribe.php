<?php
	   $visitor_email = $_POST["email"];  
	   $email_from = "sqlhuntingdog@gmail.com"; 
	   $email_subject = "New subscription "; 
	   $email_body = "New person subscribed $visitor_email.\n".

	   $to = "sqlhuntingdog@gmail.com";
	 
	   $headers = "From: $email_from \r\n";
	  
	   mail($to,$email_subject,$email_body,$headers);
	   echo "All good";
   
   	
?>