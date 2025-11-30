create table ProductionGroups
(
	pgName nvarchar(10) not null,
	includedMarks nvarchar(10) not null
)

insert into ProductionGroups ([pgName],[includedMarks])
values ('Benz-G','Smart')

select
Cars.mark as 'Марка',
Cars.model as 'Модель',
Cars.yearOfManufacture as 'Дата выпуска',
Engines.powerEngine as 'Мощность(л.с.)',
Engines.markEngine as 'Модель двигателя',
Cars.yearOfManufacture*Engines.powerEngine as 'Стоимость(руб.)'
from Cars,Engines,ProductionGroups
where Cars.mark = ProductionGroups.includedMarks
and
ProductionGroups.pgName = Engines.productionGroup
