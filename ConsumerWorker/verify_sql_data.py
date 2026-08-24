#!/usr/bin/env python3
"""
Verification script to ensure SQL seed data matches JSON source files exactly.
"""

import json
import re
import sys
from pathlib import Path

def load_json(filepath):
    """Load and return JSON data from file."""
    with open(filepath, 'r', encoding='utf-8') as f:
        return json.load(f)

def extract_sql_inserts(sql_content, table_name):
    """Extract INSERT statements for a specific table from SQL."""
    # Find the INSERT statement for the table
    pattern = rf"INSERT INTO {table_name}.*?VALUES\s*(.*?);"
    match = re.search(pattern, sql_content, re.DOTALL | re.IGNORECASE)
    
    if not match:
        return []
    
    values_section = match.group(1)
    
    # Extract individual value tuples
    # Pattern: (value1, value2, value3)
    tuple_pattern = r"\(([^)]+)\)"
    tuples = re.findall(tuple_pattern, values_section)
    
    records = []
    for t in tuples:
        # Split by comma, but respect quoted strings
        values = []
        current = ""
        in_quotes = False
        
        for char in t + ',':
            if char == "'" and (not current or current[-1] != '\\'):
                in_quotes = not in_quotes
            elif char == ',' and not in_quotes:
                values.append(current.strip().strip("'"))
                current = ""
            else:
                current += char
        
        if values:  # Remove empty last element
            records.append(values[:-1] if values[-1] == '' else values)
    
    return records

def verify_units(json_data, sql_records):
    """Verify Units table matches JSON."""
    print("\n" + "="*60)
    print("🔍 VERIFYING UNITS")
    print("="*60)
    
    errors = []
    
    if len(json_data) != len(sql_records):
        errors.append(f"❌ Count mismatch: JSON has {len(json_data)} units, SQL has {len(sql_records)}")
        return errors
    
    print(f"✓ Count matches: {len(json_data)} units")
    
    for i, json_unit in enumerate(json_data):
        if i >= len(sql_records):
            errors.append(f"❌ Missing SQL record for Unit ID {json_unit['Id']}")
            continue
            
        sql_unit = sql_records[i]
        
        # Verify Id
        if str(json_unit['Id']) != sql_unit[0]:
            errors.append(f"❌ Unit {i+1}: ID mismatch - JSON: {json_unit['Id']}, SQL: {sql_unit[0]}")
        
        # Verify UnitName
        if json_unit['UnitName'] != sql_unit[1]:
            errors.append(f"❌ Unit {json_unit['Id']}: UnitName mismatch")
            errors.append(f"   JSON: '{json_unit['UnitName']}'")
            errors.append(f"   SQL:  '{sql_unit[1]}'")
        
        # Verify Sector
        if json_unit['Sector'] != sql_unit[2]:
            errors.append(f"❌ Unit {json_unit['Id']}: Sector mismatch")
            errors.append(f"   JSON: '{json_unit['Sector']}'")
            errors.append(f"   SQL:  '{sql_unit[2]}'")
    
    if not errors:
        print("✅ All Units match perfectly!")
    
    return errors

def verify_assets(json_data, sql_records):
    """Verify Assets table matches JSON."""
    print("\n" + "="*60)
    print("🔍 VERIFYING ASSETS")
    print("="*60)
    
    errors = []
    
    if len(json_data) != len(sql_records):
        errors.append(f"❌ Count mismatch: JSON has {len(json_data)} assets, SQL has {len(sql_records)}")
        return errors
    
    print(f"✓ Count matches: {len(json_data)} assets")
    
    # Map AssetType to enum value
    type_map = {
        'UAV': '2',
        'PerimeterSensor': '1',
        'GenericAsset': '0'
    }
    
    for i, json_asset in enumerate(json_data):
        if i >= len(sql_records):
            errors.append(f"❌ Missing SQL record for Asset ID {json_asset['Id']}")
            continue
            
        sql_asset = sql_records[i]
        
        # Verify Id
        if str(json_asset['Id']) != sql_asset[0]:
            errors.append(f"❌ Asset {i+1}: ID mismatch - JSON: {json_asset['Id']}, SQL: {sql_asset[0]}")
        
        # Verify UnitId
        if str(json_asset['UnitId']) != sql_asset[1]:
            errors.append(f"❌ Asset {json_asset['Id']}: UnitId mismatch")
            errors.append(f"   JSON: {json_asset['UnitId']}, SQL: {sql_asset[1]}")
        
        # Verify AssetSerial
        if json_asset['AssetSerial'] != sql_asset[2]:
            errors.append(f"❌ Asset {json_asset['Id']}: AssetSerial mismatch")
            errors.append(f"   JSON: '{json_asset['AssetSerial']}'")
            errors.append(f"   SQL:  '{sql_asset[2]}'")
        
        # Verify Type (enum value)
        expected_type = type_map.get(json_asset['AssetType'], '2')
        if expected_type != sql_asset[3]:
            errors.append(f"❌ Asset {json_asset['Id']}: Type mismatch")
            errors.append(f"   JSON: '{json_asset['AssetType']}' (should be {expected_type})")
            errors.append(f"   SQL:  {sql_asset[3]}")
    
    if not errors:
        print("✅ All Assets match perfectly!")
    
    return errors

def main():
    print("="*60)
    print("🔍 SQL DATA VERIFICATION SCRIPT")
    print("="*60)
    
    # Paths
    base_dir = Path(__file__).parent.parent
    units_json = base_dir / 'ProducerService' / 'data' / 'units.json'
    assets_json = base_dir / 'ProducerService' / 'data' / 'assets.json'
    sql_file = Path(__file__).parent / 'seed_database.sql'
    
    # Check files exist
    for filepath in [units_json, assets_json, sql_file]:
        if not filepath.exists():
            print(f"❌ File not found: {filepath}")
            sys.exit(1)
    
    print(f"✓ Found units.json: {units_json}")
    print(f"✓ Found assets.json: {assets_json}")
    print(f"✓ Found seed_database.sql: {sql_file}")
    
    # Load JSON data
    print("\n📂 Loading JSON files...")
    units_json_data = load_json(units_json)
    assets_json_data = load_json(assets_json)
    print(f"✓ Loaded {len(units_json_data)} units from JSON")
    print(f"✓ Loaded {len(assets_json_data)} assets from JSON")
    
    # Load SQL file
    print("\n📂 Loading SQL file...")
    with open(sql_file, 'r', encoding='utf-8') as f:
        sql_content = f.read()
    
    # Extract SQL records
    print("🔍 Extracting SQL INSERT statements...")
    units_sql_records = extract_sql_inserts(sql_content, 'Units')
    assets_sql_records = extract_sql_inserts(sql_content, 'Assets')
    print(f"✓ Extracted {len(units_sql_records)} units from SQL")
    print(f"✓ Extracted {len(assets_sql_records)} assets from SQL")
    
    # Verify
    all_errors = []
    
    units_errors = verify_units(units_json_data, units_sql_records)
    all_errors.extend(units_errors)
    
    assets_errors = verify_assets(assets_json_data, assets_sql_records)
    all_errors.extend(assets_errors)
    
    # Summary
    print("\n" + "="*60)
    print("📊 VERIFICATION SUMMARY")
    print("="*60)
    
    if not all_errors:
        print("✅ ✅ ✅ ALL DATA MATCHES PERFECTLY! ✅ ✅ ✅")
        print(f"\n✓ {len(units_json_data)} Units verified")
        print(f"✓ {len(assets_json_data)} Assets verified")
        print("\n🎉 SQL seed file is 100% accurate!")
        return 0
    else:
        print(f"❌ Found {len(all_errors)} error(s):\n")
        for error in all_errors:
            print(error)
        print("\n⚠️  Please fix the SQL file and run verification again.")
        return 1

if __name__ == '__main__':
    sys.exit(main())
