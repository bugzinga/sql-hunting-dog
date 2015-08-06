				<tr>
					<td style="padding-right: 7px">
						<a href="/">
							<img src="/img/logo.jpg" width="60" height="60" />
						</a>
					</td>
					<td>
						<a href="/">
							<h1 class="mainTitle">SQL HUNTING DOG</h1>
						</a>
						<center class="menu">
<?php
    $downloadLink = "https://bitbucket.org/bugzinga/sql-hunting-dog/downloads/HuntingDog-3.3.0.msi";
	$menu = array(
		array(
			"name" => "main",
			"link" => "/",
			"title" => "What"
		),
		array(
			"name" => "why",
			"link" => "/why",
			"title" => "Why"
		),
		array(
			"name" => "how",
			"link" => "/how",
			"title" => "How"
		),
		array(
			"name" => "source",
			"link" => "http://bitbucket.org/bugzinga/sql-hunting-dog/",
			"title" => "Source"
		),
		array(
			"name" => "who",
			"link" => "/who",
			"title" => "Who"
		)				
	);

	foreach ($menu as $item) {
		if ($active == $item["name"]) {
?>
							<span class="active"><?php echo $item["title"] ?></span>
<?php
		} else {
?>
							<a href="<?php echo $item['link']?>" class="inactive"><?php echo $item["title"] ?></a>
<?php
		}
	}
?>
						</center>
					</td>
					
					
					
				</tr>
