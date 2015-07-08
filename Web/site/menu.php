				<tr>
					<td style="padding-right: 7px">
						<a href="/">
							<img src="/img/logo.jpg" width="60" height="60" />
						</a>
					</td>
					<td>
						<a href="/">
							<h1>SQL HUNTING DOG</h1>
						</a>
						<center class="menu">
<?php
    $downloadLink = "https://bitbucket.org/bugzinga/sql-hunting-dog/downloads/HuntingDog-3.2.2.msi";
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
			"name" => "download",
			"link" => $downloadLink,
			"title" => "Download"
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
					<td style="padding-left: 11px">
						<a href="<?php echo $downloadLink ?>">
							<img src="/img/download.png" width="50" height="50" /><br/>
						</a>
					</td>
				</tr>
